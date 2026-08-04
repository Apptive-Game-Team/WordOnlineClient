using System;
using System.Collections.Generic;
using Data;
using Data.Magic;
using Data.GameConfig;
using FixMath.NET;
using GameScene.Card;
using GameScene.Simulation;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Mob;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Pve;
using GameScene.Simulation.Rendering;
using GameScene.Simulation.Resources;
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
        private DeterministicMatchSimulation match;
        private PracticeBot practiceBot;
        private PveScriptEventPresenter pvePresenter;
        private long localUserId;
        private bool configured;
        private bool spectator;
        private CardInputSender cardInputSender;
        private readonly List<string> localSelectionEffects = new List<string>();

        public SimulationWorld World => world;
        public event Action<PveScriptEvent> PveScriptEventFired;

        public void Configure(StompConnector value)
        {
            if (configured) return;
            connector = value != null ? value : throw new ArgumentNullException(nameof(value));
            rendererBridge = FindObjectOfType<SimulationRendererBridge>() ?? gameObject.AddComponent<SimulationRendererBridge>();
            if (!connector.IsSpectator)
            {
                GameSceneUIController uiController = GameSceneUIController.Instance;
                if (uiController == null)
                    throw new InvalidOperationException("GameSceneUIController is required for a lockstep player");
                SimulationPlayerUiAdapter uiAdapter = FindObjectOfType<SimulationPlayerUiAdapter>() ??
                    gameObject.AddComponent<SimulationPlayerUiAdapter>();
                uiAdapter.Configure(uiController);
                rendererBridge.ConfigurePlayerUi(uiAdapter);
            }
            pvePresenter = GetComponent<PveScriptEventPresenter>() ??
                gameObject.AddComponent<PveScriptEventPresenter>();
            pvePresenter.Configure(rendererBridge);
            connector.LockstepSessionStarted += StartSession;
            configured = true;
        }

        private void OnDestroy()
        {
            if (connector != null) connector.LockstepSessionStarted -= StartSession;
            if (cardInputSender != null) cardInputSender.SelectionChanged -= HandleSelectionChanged;
        }

        private void Update()
        {
            if (connector == null || world == null) return;
            while (connector.TryDequeueConfirmedFrame(out ConfirmedFrameMessage frame))
                ProcessFrame(frame, !spectator);
        }

        public void StartSession(LockstepSessionStartMessage start)
        {
            LockstepVersionValidator.ValidateDataVersions(start,
                ParametersDataSource.GetCachedVersion(), MagicInfoDataSource.GetCachedVersion());
            spectator = connector.IsSpectator;
            localUserId = SceneContext.UserID;
            cardInputSender = spectator ? null : CardInputSender.Instance;
            if (cardInputSender != null)
            {
                cardInputSender.SelectionChanged -= HandleSelectionChanged;
                cardInputSender.SelectionChanged += HandleSelectionChanged;
            }
            rendererBridge.ConfigureParticipants(start.leftUserId, start.rightUserId);
            DeterministicGameRules rules = ProductionResourceRulesFactory.Create();
            PveSessionSimulation pve = null;
            SimulationPrefabRegistry prefabs = ProductionSimulationPrefabRegistryFactory.Create();
            if (string.Equals(start.sessionType, "PVE", StringComparison.Ordinal))
            {
                pve = PveSessionSimulation.Start(start,
                    ProductionPveScenarioCatalog.Create(rules.DurationFrames), prefabs);
                world = pve.World;
            }
            else
            {
                world = new SimulationWorld(start.rngSeed, prefabs);
                world.ApplyBootstrap(start);
            }
            ResourceSimulation resources = new ResourceSimulation(
                rules, start.leftUserId, start.rightUserId,
                start.leftCards ?? Array.Empty<string>(), start.rightCards ?? Array.Empty<string>());
            ProductionMagicCatalog magicCatalog = ProductionMagicCatalogFactory.Create();
            MagicSimulation magic = new MagicSimulation(start.rngSeed, world, magicCatalog);
            MobSimulation mobs = new MobSimulation(world, new NavigationGrid(18, 10));
            RegisterPlayersAsDamageableMobs(start, mobs);
            match = new DeterministicMatchSimulation(
                resources, magic, mobs, pve, ProductionMobDefinitionResolverFactory.Create());
            long botId = start.leftUserId < 0 ? start.leftUserId : start.rightUserId < 0 ? start.rightUserId : 0;
            string[] botCards = botId == start.leftUserId ? start.leftCards : start.rightCards;
            practiceBot = CreatePracticeBot(start, botId, botCards, rules, magicCatalog);
            Render();
            if (!spectator)
                connector.SendFrameSubmission(connector.CreateFrameSubmission(start.initialFrame,
                    CalculateSessionHash().ToString("X16")));
        }

        private static void RegisterPlayersAsDamageableMobs(LockstepSessionStartMessage start, MobSimulation mobs)
        {
            MobDefinition player = new MobDefinition(
                Fix64.Zero, Fix64.Zero, Fix64.Zero, 0, 1, 20, 100, Fix64.One, MobArchetype.Dummy);
            for (int index = 0; index < mobs.World.Entities.Count; index++)
            {
                SimulationEntity entity = mobs.World.Entities[index];
                if (!entity.IsDestroyed && entity.PrefabId == SimulationPrefabRegistry.PlayerPrefabId &&
                    (entity.OwnerUserId == start.leftUserId || entity.OwnerUserId == start.rightUserId))
                    mobs.Register(entity.Id, player);
            }
        }

        public void ProcessFrame(ConfirmedFrameMessage frame, bool submitNextFrame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (match == null) throw new InvalidOperationException("Lockstep session has not started");
            List<ConfirmedInputMessage> inputs = new List<ConfirmedInputMessage>(frame.inputs ?? Array.Empty<ConfirmedInputMessage>());
            IReadOnlyList<string> botHand = null;
            if (practiceBot != null)
                botHand = match.Resources.Left.UserId < 0
                    ? match.Resources.Left.Hand : match.Resources.Right.Hand;
            ConfirmedInputMessage botInput = null;
            if (practiceBot != null)
            {
                PlayerResourceState botResources = match.Resources.Left.UserId < 0
                    ? match.Resources.Left : match.Resources.Right;
                botInput = practiceBot.Decide(frame.frameNum, world, botHand, botResources.Mana);
            }
            if (botInput != null) inputs.Add(botInput);
            inputs.Sort((left, right) => { int user = left.userId.CompareTo(right.userId); return user != 0 ? user : left.input.sequence.CompareTo(right.input.sequence); });
            List<MagicCastCommand> casts = new List<MagicCastCommand>();
            for (int index = 0; index < inputs.Count; index++)
            {
                ConfirmedInputMessage confirmed = inputs[index]; FrameInputMessage input = confirmed.input;
                if (input == null) continue;
                if (string.Equals(input.type, "setCardSelection", StringComparison.Ordinal)) continue;
                if (!string.Equals(input.type, "useMagic", StringComparison.Ordinal))
                    throw new InvalidOperationException("Unsupported confirmed gameplay input: " + input.type);
                if (TryToMagicCast(confirmed, out MagicCastCommand cast)) casts.Add(cast);
            }
            ConfirmedFrameMessage combinedFrame = new ConfirmedFrameMessage
            {
                type = frame.type,
                protocolVersion = frame.protocolVersion,
                frameNum = frame.frameNum,
                inputs = inputs.ToArray(),
                hashMatched = frame.hashMatched
            };
            match.Step(combinedFrame, casts);
            if (match.Pve != null)
                for (int index = 0; index < match.Pve.FiredThisFrame.Count; index++)
                {
                    PveScriptEvent scriptEvent = match.Pve.FiredThisFrame[index];
                    pvePresenter?.Present(scriptEvent);
                    PveScriptEventFired?.Invoke(scriptEvent);
                }
            ConfirmLocalInputs();
            Render();
            if (!submitNextFrame) return;
            string stateHash = CalculateSessionHash().ToString("X16");
            if (match.Result != SimulationResult.Ongoing)
            {
                connector.SendResultSubmission(LockstepResultSubmissionMessage.Create(
                    frame.frameNum, stateHash, match.Result));
                return;
            }
            connector.SendFrameSubmission(connector.CreateFrameSubmission(frame.frameNum + 1, stateHash));
        }

        private bool TryToMagicCast(ConfirmedInputMessage confirmed, out MagicCastCommand cast)
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
            {
                cast = default;
                return false;
            }
            ProtocolVector3 position = input.position ?? throw new InvalidOperationException("Confirmed magic position missing");
            cast = new MagicCastCommand(confirmed.userId, input.id, data.serverName,
                new SimVector2((Fix64)(decimal)position.x, (Fix64)(decimal)position.z));
            return true;
        }

        private void Render()
        {
            if (rendererBridge == null) rendererBridge = FindObjectOfType<SimulationRendererBridge>();
            if (rendererBridge == null) throw new InvalidOperationException("SimulationRendererBridge is required in GameScene");
            PlayerResourceSnapshot player = match != null && !spectator
                ? match.Resources.CreatePlayerSnapshot(localUserId) : null;
            rendererBridge.ApplySnapshot(match != null ? match.CreateSnapshot() : world.CreateSnapshot(), player);
            if (!spectator) rendererBridge.ApplyLocalPlayerEffects(localUserId, localSelectionEffects);
        }

        private void ConfirmLocalInputs()
        {
            IReadOnlyList<CastResult> results = match.Resources.LastCastResults;
            for (int index = 0; index < results.Count; index++)
            {
                CastResult result = results[index];
                if (result.UserId != localUserId) continue;
                CardInputSender.Instance?.HandleConfirmedInput(result.RequestId,
                    result.Code == CastResultCode.Success, result.Code.ToString());
            }
        }

        private static IEnumerable<PracticeBotRecipe> BotRecipes(
            DeterministicGameRules rules, ProductionMagicCatalog catalog)
        {
            foreach (CombinedMagicData magic in LocalCombinedMagicData.GetEffectiveDataList())
            {
                List<string> cards = new List<string>(magic.recipe.Count);
                int cost = 0;
                bool offensive = false;
                for (int index = 0; index < magic.recipe.Count; index++)
                {
                    string card = magic.recipe[index].ToString();
                    cards.Add(card);
                    cost = checked(cost + rules.ManaCost(card));
                    if (card == "Shoot" || card == "Explode") offensive = true;
                }
                yield return new PracticeBotRecipe(cards,
                    catalog.GetRequired(magic.serverName).Range, cost, offensive);
            }
        }

        private static IReadOnlyDictionary<string, int> CardCosts(DeterministicGameRules rules)
        {
            string[] names =
            {
                "Shoot", "Build", "Spawn", "Explode", "Drop",
                "Fire", "Water", "Lightning", "Rock", "Nature", "Wind"
            };
            Dictionary<string, int> costs = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int index = 0; index < names.Length; index++)
                costs.Add(names[index], rules.ManaCost(names[index]));
            return costs;
        }

        private static PracticeBot CreatePracticeBot(LockstepSessionStartMessage start,
            long botId, string[] botCards, DeterministicGameRules rules,
            ProductionMagicCatalog catalog)
        {
            if (botId >= 0 || botCards == null || botCards.Length == 0) return null;
            LockstepBotConfigMessage config = start.botConfig;
            if (config == null || config.participantId != botId ||
                config.reactionIntervalFrames < 1 || config.thinkingDelayFrames < 0)
                throw new InvalidOperationException("Valid lockstep bot config is required");
            return new PracticeBot(start.rngSeed, botId, config.reactionIntervalFrames,
                config.thinkingDelayFrames, BotRecipes(rules, catalog), botCards, CardCosts(rules),
                null, config.tier, config.counterAggression);
        }

        private ulong CalculateSessionHash()
        {
            CanonicalStateWriter writer = new CanonicalStateWriter();
            writer.WriteUInt64(match.CalculateStateHash());
            writer.WriteBoolean(practiceBot != null);
            if (practiceBot != null) writer.WriteUInt64(practiceBot.CalculateStateHash());
            return writer.Hash;
        }

        private void HandleSelectionChanged(IReadOnlyList<CardType> cards)
        {
            localSelectionEffects.Clear();
            if (cards != null)
                for (int index = 0; index < cards.Count; index++)
                {
                    string effect = IdleAuraEffect(cards[index]);
                    if (effect != null && !localSelectionEffects.Contains(effect))
                        localSelectionEffects.Add(effect);
                }
            rendererBridge?.ApplyLocalPlayerEffects(localUserId, localSelectionEffects);
            if (!spectator)
            {
                string[] selected = new string[cards?.Count ?? 0];
                for (int index = 0; index < selected.Length; index++)
                    selected[index] = cards[index].ToString();
                connector.QueueCardSelection(selected);
            }
        }

        private static string IdleAuraEffect(CardType card)
        {
            switch (card)
            {
                case CardType.Fire: return "FireIdleAura";
                case CardType.Water: return "WaterIdleAura";
                case CardType.Nature: return "NatureIdleAura";
                case CardType.Lightning: return "LightningIdleAura";
                case CardType.Rock: return "RockIdleAura";
                case CardType.Wind: return "WindIdleAura";
                default: return null;
            }
        }

    }
}
