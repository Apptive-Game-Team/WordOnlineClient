using System;
using System.Collections.Generic;

namespace Simulation.Core
{
    /// <summary>
    /// Main deterministic simulation world.
    /// Owns all game objects, systems, physics, RNG, player data, and card decks.
    /// Both clients run identical Step() calls with the same inputs to stay in sync.
    /// </summary>
    public class SimWorld
    {
        public SimRandom Random { get; private set; }
        public SimPhysics Physics { get; private set; }
        public int FrameNum { get; private set; }

        // Player state
        public SimPlayerData LeftPlayerData { get; private set; }
        public SimPlayerData RightPlayerData { get; private set; }
        public SimCardDeck LeftCardDeck { get; private set; }
        public SimCardDeck RightCardDeck { get; private set; }

        // Systems
        private SimPhysicSystem _physicSystem;
        private SimMagicInputSystem _magicInputSystem;
        private SimMagicRegistry _magicRegistry;

        // Game result
        public Master? Loser { get; private set; }
        public bool IsGameOver => Loser != null;

        private readonly List<SimGameObject> _gameObjects = new();
        private readonly List<SimGameObject> _pendingStart = new();
        private readonly HashSet<SimGameObject> _updated = new();

        /// <summary>
        /// Called by rendering layer to sync visual state.
        /// </summary>
        public event Action<SimGameObject> OnObjectCreated;
        public event Action<SimGameObject> OnObjectUpdated;
        public event Action<SimGameObject> OnObjectDestroyed;

        /// <summary>
        /// Initialize the simulation world from sessionStart data.
        /// </summary>
        public void Init(long rngSeed,
            List<SimCardType> leftCards, List<SimCardType> rightCards,
            Dictionary<string, Dictionary<string, Fix64>> parameters)
        {
            SimGameObject.ResetIdCounter();
            Random = new SimRandom(rngSeed);
            Physics = new SimPhysics(_gameObjects);
            FrameNum = 0;
            Loser = null;
            _gameObjects.Clear();
            _pendingStart.Clear();
            _updated.Clear();

            // Parameters
            SimPrefabFactory.SetParameters(parameters);

            // Player data
            LeftPlayerData = new SimPlayerData(parameters);
            RightPlayerData = new SimPlayerData(parameters);

            // Card decks (shuffled deterministically with the same RNG)
            LeftCardDeck = new SimCardDeck(leftCards, Random);
            RightCardDeck = new SimCardDeck(rightCards, Random);

            // Systems
            _physicSystem = new SimPhysicSystem();
            // Build magic registry from server recipe data if available
            var serverRecipes = Data.GameConfig.GameDataManager.Config?.magicRecipes;
            _magicRegistry = SimMagicRegistry.BuildFromServerData(serverRecipes);
            _magicInputSystem = new SimMagicInputSystem(_magicRegistry, parameters);

            // Create player objects and center obstacle
            new SimGameObject(Master.LeftPlayer, PrefabType.Player,
                SimGameConfig.LEFT_PLAYER_POSITION, this);
            new SimGameObject(Master.RightPlayer, PrefabType.Player,
                SimGameConfig.RIGHT_PLAYER_POSITION, this);
            new SimGameObject(Master.None, PrefabType.Wall,
                new SimVector3(SimGameConfig.OBSTACLE_CENTER.X, SimGameConfig.OBSTACLE_CENTER.Y, Fix64.Zero), this);
        }

        /// <summary>
        /// Advance one simulation frame with confirmed inputs from both players.
        /// </summary>
        public void Step(Dictionary<long, SimInputRequest> inputs,
            long leftUserId, long rightUserId)
        {
            if (IsGameOver) return;

            // 0. Update player resources (Mana regen)
            Fix64 deltaTime = (Fix64)0.05; // 20 FPS (50ms)
            LeftPlayerData.RegenerateMana(deltaTime);
            RightPlayerData.RegenerateMana(deltaTime);

            // 1. Draw cards for players
            LeftCardDeck.DrawCard(LeftPlayerData, FrameNum);
            RightCardDeck.DrawCard(RightPlayerData, FrameNum);

            // 2. Process magic inputs
            if (inputs != null)
            {
                foreach (var (userId, input) in inputs)
                {
                    Master master;
                    SimPlayerData playerData;
                    SimCardDeck cardDeck;

                    if (userId == leftUserId)
                    {
                        master = Master.LeftPlayer;
                        playerData = LeftPlayerData;
                        cardDeck = LeftCardDeck;
                    }
                    else if (userId == rightUserId)
                    {
                        master = Master.RightPlayer;
                        playerData = RightPlayerData;
                        cardDeck = RightCardDeck;
                    }
                    else continue;

                    _magicInputSystem.HandleInput(this, master, playerData, cardDeck,
                        input.Cards, input.Position);
                }
            }

            // 3. Start any newly registered objects
            if (_pendingStart.Count > 0)
            {
                var toStart = new List<SimGameObject>(_pendingStart);
                _pendingStart.Clear();
                foreach (var obj in toStart)
                    obj.Start();
            }

            // 4. Update all non-destroyed objects (component updates)
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var obj = _gameObjects[i];
                if (obj.Status != SimStatus.Destroyed)
                {
                    obj.Update();
                    MarkUpdated(obj); // Ensure visuals are synced every frame
                }
            }

            // 5. Physics (collision detection + response)
            _physicSystem.Update(this);

            // 6. Remove destroyed objects + check for game over
            for (int i = _gameObjects.Count - 1; i >= 0; i--)
            {
                var obj = _gameObjects[i];
                if (obj.IsDestroyed)
                {
                    // Check if destroyed player
                    if (obj.Type == PrefabType.Player && Loser == null)
                        Loser = obj.Master;

                    OnObjectDestroyed?.Invoke(obj);
                    _gameObjects.RemoveAt(i);
                }
            }

            _updated.Clear();
            FrameNum++;
        }

        // ── Object management (called by SimGameObject constructor) ──

        public void RegisterGameObject(SimGameObject obj)
        {
            _gameObjects.Add(obj);
            _pendingStart.Add(obj);
            OnObjectCreated?.Invoke(obj);
        }

        public void MarkUpdated(SimGameObject obj)
        {
            if (_updated.Add(obj))
                OnObjectUpdated?.Invoke(obj);
        }

        // ── Queries ──

        public List<SimGameObject> GetGameObjects() => _gameObjects;

        public List<SimGameObject> GetActiveGameObjects()
        {
            var result = new List<SimGameObject>();
            foreach (var obj in _gameObjects)
                if (obj.IsActive) result.Add(obj);
            return result;
        }

        public SimGameObject FindById(int id)
        {
            foreach (var obj in _gameObjects)
                if (obj.Id == id) return obj;
            return null;
        }

        public List<SimGameObject> FindByMaster(Master master)
        {
            var result = new List<SimGameObject>();
            foreach (var obj in _gameObjects)
                if (obj.Master == master && obj.IsActive) result.Add(obj);
            return result;
        }

        public List<SimGameObject> FindByType(PrefabType type)
        {
            var result = new List<SimGameObject>();
            foreach (var obj in _gameObjects)
                if (obj.Type == type && obj.IsActive) result.Add(obj);
            return result;
        }

        public List<SimGameObject> OverlapSphereAll(SimGameObject obj, Fix64 radius)
            => Physics.OverlapSphereAll(obj, radius);

        public List<SimGameObject> OverlapSphereAll(SimVector3 pos, Fix64 radius)
            => Physics.OverlapSphereAll(pos, radius);

        public SimPlayerData GetPlayerData(Master master) => master == Master.LeftPlayer
            ? LeftPlayerData : RightPlayerData;

        /// <summary>
        /// Spawns an object directly into the simulation (PVE initial objects).
        /// Returns the new object's ID so callers can map installerId → objectId.
        /// Must be called after Init() and before the first Step().
        /// </summary>
        public int SpawnObject(PrefabType type, Master master, SimVector3 position)
        {
            var obj = new SimGameObject(master, type, position, this);
            return obj.Id;
        }
    }
}
