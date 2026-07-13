using System;
using System.Collections.Generic;
using Data;
using Data.Magic;
using FixMath.NET;
using GameScene.Card;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Pve;
using GameScene.Simulation.Rendering;
using Global;
using Global.Util;
using UnityEngine;

namespace GameScene.Lockstep
{
    public sealed class LockstepGameController : MonoBehaviour
    {
        private StompConnector connector;
        private SimulationRendererBridge rendererBridge;
        private SimulationWorld world;
        private MagicSimulation magic;
        private PracticeBot practiceBot;
        private long localUserId;
        private bool configured;

        public SimulationWorld World => world;

        public void Configure(StompConnector value)
        {
            if (configured) return;
            connector = value != null ? value : throw new ArgumentNullException(nameof(value));
            rendererBridge = FindObjectOfType<SimulationRendererBridge>() ?? gameObject.AddComponent<SimulationRendererBridge>();
            connector.LockstepSessionStarted += StartSession;
            configured = true;
        }

        private void OnDestroy()
        {
            if (connector != null) connector.LockstepSessionStarted -= StartSession;
        }

        private void Update()
        {
            if (connector == null || world == null) return;
            while (connector.TryDequeueConfirmedFrame(out ConfirmedFrameMessage frame)) ProcessFrame(frame, true);
        }

        public void StartSession(LockstepSessionStartMessage start)
        {
            LockstepVersionValidator.Validate(start);
            localUserId = SceneContext.UserID;
            if (string.Equals(start.sessionType, "PVE", StringComparison.Ordinal))
                world = PveSessionSimulation.Start(start, ProductionPveScenarioCatalog.Create()).World;
            else
            {
                world = new SimulationWorld(start.rngSeed, SimulationPrefabRegistry.CreateProduction());
                world.ApplyBootstrap(start);
            }
            magic = new MagicSimulation(start.rngSeed, world, ProductionMagicCatalog.Create());
            long botId = start.leftUserId < 0 ? start.leftUserId : start.rightUserId < 0 ? start.rightUserId : 0;
            string[] botCards = botId == start.leftUserId ? start.leftCards : start.rightCards;
            practiceBot = botId < 0 && botCards != null && botCards.Length > 0
                ? new PracticeBot(start.rngSeed, botId, 10, botCards) : null;
            Render();
            connector.SendFrameSubmission(connector.CreateFrameSubmission(start.initialFrame,
                magic.CalculateStateHash().ToString("X16")));
        }

        public void ProcessFrame(ConfirmedFrameMessage frame, bool submitNextFrame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (magic == null) throw new InvalidOperationException("Lockstep session has not started");
            List<ConfirmedInputMessage> inputs = new List<ConfirmedInputMessage>(frame.inputs ?? Array.Empty<ConfirmedInputMessage>());
            ConfirmedInputMessage botInput = practiceBot?.Decide(frame.frameNum, world);
            if (botInput != null) inputs.Add(botInput);
            inputs.Sort((left, right) => { int user = left.userId.CompareTo(right.userId); return user != 0 ? user : left.input.sequence.CompareTo(right.input.sequence); });
            List<MagicCastCommand> casts = new List<MagicCastCommand>();
            for (int index = 0; index < inputs.Count; index++)
            {
                ConfirmedInputMessage confirmed = inputs[index]; FrameInputMessage input = confirmed.input;
                if (input == null) continue;
                if (!string.Equals(input.type, "useMagic", StringComparison.Ordinal))
                    throw new InvalidOperationException("Unsupported confirmed gameplay input: " + input.type);
                casts.Add(ToMagicCast(confirmed));
            }
            magic.Step(casts);
            for (int index = 0; index < inputs.Count; index++)
                if (inputs[index].userId == localUserId && inputs[index].input != null)
                    CardInputSender.Instance?.HandleConfirmedInput(inputs[index].input.id, true);
            Render();
            if (submitNextFrame)
                connector.SendFrameSubmission(connector.CreateFrameSubmission(frame.frameNum + 1,
                    magic.CalculateStateHash().ToString("X16")));
        }

        private MagicCastCommand ToMagicCast(ConfirmedInputMessage confirmed)
        {
            FrameInputMessage input = confirmed.input; List<CardType> recipe = new List<CardType>();
            string[] cards = input.cards ?? Array.Empty<string>();
            for (int index = 0; index < cards.Length; index++)
            {
                if (!CardNameMapper.TryMapToCardType(cards[index], out CardType card))
                    throw new InvalidOperationException("Unknown confirmed card: " + cards[index]);
                recipe.Add(card);
            }
            if (!LocalCombinedMagicData.TryGetByRecipe(recipe, out CombinedMagicData data))
                throw new InvalidOperationException("Confirmed magic recipe missing from versioned config");
            ProtocolVector3 position = input.position ?? throw new InvalidOperationException("Confirmed magic position missing");
            return new MagicCastCommand(confirmed.userId, input.id, checked((int)data.id),
                new SimVector2((Fix64)(decimal)position.x, (Fix64)(decimal)position.z));
        }

        private void Render()
        {
            if (rendererBridge == null) rendererBridge = FindObjectOfType<SimulationRendererBridge>();
            if (rendererBridge == null) throw new InvalidOperationException("SimulationRendererBridge is required in GameScene");
            rendererBridge.ApplySnapshot(world.CreateSnapshot());
        }
    }
}
