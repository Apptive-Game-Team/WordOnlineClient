using System.Collections;
using System.Collections.Generic;
using FixMath.NET;
using GameScene.Simulation.Core;
using GameScene.Simulation.Magic;
using GameScene.Simulation.Objects;
using GameScene.Simulation.Protocol;
using GameScene.Simulation.Resources;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameScene.Simulation.Rendering.Tests
{
    public sealed class PlayerUiSpy : MonoBehaviour, ISimulationPlayerUi
    {
        public int Mana { get; private set; } = -1;
        public int HandCount { get; private set; } = -1;
        public int RemainingFrames { get; private set; } = -1;

        public void Render(PlayerResourceSnapshot snapshot)
        {
            Mana = snapshot.Mana;
            HandCount = snapshot.Hand.Count;
            RemainingFrames = snapshot.RemainingFrames;
        }
    }

    public sealed class EntityViewSpy : MonoBehaviour, ISimulationEntityView
    {
        public string Status { get; private set; }
        public string Master { get; private set; }
        public int GaugeCount { get; private set; }
        public int EffectCount { get; private set; }

        public void ApplySimulationState(string status, IReadOnlyList<string> effects,
            IReadOnlyList<SimulationGaugeSnapshot> gauges, string master)
        {
            Status = status;
            Master = master;
            GaugeCount = gauges?.Count ?? 0;
            EffectCount = effects?.Count ?? 0;
        }

        public void ApplyLocalEffects(IReadOnlyList<string> effects) =>
            EffectCount = effects?.Count ?? 0;

        public int BindCount { get; private set; }
        public int SpawnCount { get; private set; }
        public int MoveCount { get; private set; }

        public void BindListeners() => BindCount++;

        public void NotifySpawned() => SpawnCount++;

        public void NotifyMoved() => MoveCount++;
    }

    public sealed class SimulationRendererBridgeTests
    {
        [Test]
        public void EveryProductionMagicRootPrefabExistsInResources()
        {
            foreach (MagicDefinition definition in ProductionMagicCatalog.Create().Definitions)
                Assert.That(UnityEngine.Resources.Load<GameObject>("Prefabs/" +
                    SimulationRendererBridge.ResolveResourcePrefabId(definition.PrefabId)), Is.Not.Null,
                    definition.Id + ":" + definition.Name + " -> " + definition.PrefabId);
        }

        [Test]
        public void EveryServerPrefabHasProductionRendererResource()
        {
            foreach (string prefabId in ServerPrefabManifest.All)
            {
                string resourceId = SimulationRendererBridge.ResolveResourcePrefabId(prefabId);
                if (resourceId == null)
                {
                    Assert.That(prefabId, Is.EqualTo("Wall"));
                    continue;
                }
                Assert.That(UnityEngine.Resources.Load<GameObject>("Prefabs/" + resourceId),
                    Is.Not.Null, prefabId + " -> " + resourceId);
            }
        }

        [UnityTest]
        public IEnumerator SpawnUpdateDestroyAndReloadLeaveNoStaleBindings()
        {
            GameObject prefab = new GameObject("PlayerViewPrefab");
            prefab.SetActive(false);
            GameObject host = new GameObject("RendererBridge");
            SimulationRendererBridge bridge = host.AddComponent<SimulationRendererBridge>();
            bridge.ConfigureForTests(new[]
            {
                new SimulationRendererBridge.PrefabBinding { prefabId = "Default", prefab = prefab }
            });
            SimulationWorld world = new SimulationWorld(1);
            world.Spawn(10, SimVector2.Zero);
            ulong simulationHash = world.CalculateStateHash();

            bridge.ApplySnapshot(world.CreateSnapshot());
            Assert.That(bridge.BindingCount, Is.EqualTo(1));
            GameObject view = bridge.FindView(0);
            Assert.That(view, Is.Not.Null);
            Assert.That(world.CalculateStateHash(), Is.EqualTo(simulationHash));

            world.Step(new[]
            {
                new SimulationInput(10, 0, SimulationInputType.SetVelocity, 0,
                    new SimVector2((Fix64)10, Fix64.Zero))
            });
            bridge.ApplySnapshot(world.CreateSnapshot());
            yield return null;
            Assert.That(view.transform.position.x, Is.GreaterThan(0f));

            world.Destroy(0);
            bridge.ApplySnapshot(world.CreateSnapshot());
            Assert.That(bridge.BindingCount, Is.EqualTo(0));
            yield return null;
            Assert.That(view == null, Is.True);

            Object.Destroy(host);
            yield return null;
            GameObject reloadedHost = new GameObject("RendererBridgeReloaded");
            SimulationRendererBridge reloaded = reloadedHost.AddComponent<SimulationRendererBridge>();
            reloaded.ConfigureForTests(new[]
            {
                new SimulationRendererBridge.PrefabBinding { prefabId = "Default", prefab = prefab }
            });
            Assert.That(reloaded.BindingCount, Is.EqualTo(0));

            Object.Destroy(reloadedHost);
            Object.Destroy(prefab);
        }

        [UnityTest]
        public IEnumerator PlayerUiRendersFromDetachedResourceSnapshot()
        {
            GameObject host = new GameObject("RendererBridge");
            SimulationRendererBridge bridge = host.AddComponent<SimulationRendererBridge>();
            PlayerUiSpy ui = host.AddComponent<PlayerUiSpy>();
            bridge.ConfigurePlayerUiForTests(ui);
            ResourceSimulation resources = new ResourceSimulation(
                new DeterministicGameRules(
                    10, 1, 1, 2, 1, 5,
                    new Dictionary<string, int> { ["Fire"] = 1 },
                    new[] { new[] { "Fire" } }),
                10, 20, new[] { "Fire" }, new[] { "Fire" });
            resources.Step(new ConfirmedFrameMessage
            {
                frameNum = 1,
                inputs = System.Array.Empty<ConfirmedInputMessage>()
            });
            PlayerResourceSnapshot playerSnapshot = resources.CreatePlayerSnapshot(10);
            SimulationWorld world = new SimulationWorld(1);

            bridge.ApplySnapshot(world.CreateSnapshot(), playerSnapshot);

            Assert.That(ui.Mana, Is.EqualTo(4));
            Assert.That(ui.HandCount, Is.EqualTo(1));
            Assert.That(ui.RemainingFrames, Is.EqualTo(4));
            Object.Destroy(host);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EntityPresentationUsesAsmdefSafeViewContract()
        {
            GameObject prefab = new GameObject("PlayerViewPrefab");
            prefab.SetActive(false);
            prefab.AddComponent<EntityViewSpy>();
            GameObject host = new GameObject("RendererBridge");
            SimulationRendererBridge bridge = host.AddComponent<SimulationRendererBridge>();
            bridge.ConfigureParticipants(10, 20);
            bridge.ConfigureForTests(new[]
            {
                new SimulationRendererBridge.PrefabBinding { prefabId = "Default", prefab = prefab }
            });
            SimulationWorld world = new SimulationWorld(1);
            world.Spawn(10, SimVector2.Zero);

            bridge.ApplySnapshot(world.CreateSnapshot());
            EntityViewSpy view = bridge.FindView(0).GetComponent<EntityViewSpy>();

            Assert.That(view.Master, Is.EqualTo("LeftPlayer"));
            Assert.That(view.Status, Is.EqualTo("Idle"));
            Object.Destroy(host);
            Object.Destroy(prefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PrefabPresentationBindsOnSpawnAndReportsMovement()
        {
            GameObject prefab = new GameObject("PlayerViewPrefab");
            prefab.SetActive(false);
            prefab.AddComponent<EntityViewSpy>();
            GameObject host = new GameObject("RendererBridge");
            SimulationRendererBridge bridge = host.AddComponent<SimulationRendererBridge>();
            bridge.ConfigureParticipants(10, 20);
            bridge.ConfigureForTests(new[]
            {
                new SimulationRendererBridge.PrefabBinding { prefabId = "Default", prefab = prefab }
            });
            SimulationWorld world = new SimulationWorld(1);
            world.Spawn(10, SimVector2.Zero);

            bridge.ApplySnapshot(world.CreateSnapshot());
            EntityViewSpy view = bridge.FindView(0).GetComponent<EntityViewSpy>();
            Assert.That(view.BindCount, Is.EqualTo(1));
            Assert.That(view.SpawnCount, Is.EqualTo(1));
            Assert.That(view.MoveCount, Is.EqualTo(0));

            // A standing entity must stay silent, or movement sfx retriggers every snapshot.
            bridge.ApplySnapshot(world.CreateSnapshot());
            Assert.That(view.BindCount, Is.EqualTo(1));
            Assert.That(view.SpawnCount, Is.EqualTo(1));
            Assert.That(view.MoveCount, Is.EqualTo(0));

            world.Step(new[]
            {
                new SimulationInput(10, 0, SimulationInputType.SetVelocity, 0,
                    new SimVector2((Fix64)10, Fix64.Zero))
            });
            bridge.ApplySnapshot(world.CreateSnapshot());
            Assert.That(view.MoveCount, Is.EqualTo(1));
            Assert.That(view.SpawnCount, Is.EqualTo(1));

            Object.Destroy(host);
            Object.Destroy(prefab);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MissingPrefabFailsWithoutCreatingBinding()
        {
            GameObject host = new GameObject("RendererBridge");
            SimulationRendererBridge bridge = host.AddComponent<SimulationRendererBridge>();
            SimulationWorld world = new SimulationWorld(1);
            world.Spawn(10, SimVector2.Zero);

            Assert.Throws<System.InvalidOperationException>(() => bridge.ApplySnapshot(world.CreateSnapshot()));
            Assert.That(bridge.BindingCount, Is.EqualTo(0));

            Object.Destroy(host);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PhysicsOnlyWallDoesNotCreateRendererBinding()
        {
            GameObject host = new GameObject("RendererBridge");
            SimulationRendererBridge bridge = host.AddComponent<SimulationRendererBridge>();
            SimulationWorld world = new SimulationWorld(1,
                SimulationPrefabRegistry.CreateProduction());
            SimulationEntity wall = world.Spawn("Wall", 0, SimVector2.Zero);

            Assert.DoesNotThrow(() => bridge.ApplySnapshot(world.CreateSnapshot()));
            Assert.That(bridge.BindingCount, Is.EqualTo(0));
            Assert.That(bridge.FindView(wall.Id), Is.Null);

            Object.Destroy(host);
            yield return null;
        }
    }
}
