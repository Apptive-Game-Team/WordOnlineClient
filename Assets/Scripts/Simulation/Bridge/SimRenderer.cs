using System.Collections.Generic;
using Data;
using GameScene.Object;
using GameScene.ServedObjectComponent;
using Global;
using Simulation.Core;
using UnityEngine;

namespace Simulation.Bridge
{
    /// <summary>
    /// Bridges SimWorld events to Unity GameObjects.
    /// Listens to OnObjectCreated/Updated/Destroyed and drives
    /// ObjectSpawner, ObjectUpdater, and ObjectContainer.
    /// </summary>
    public class SimRenderer
    {
        private readonly SimWorld _world;

        public SimRenderer(SimWorld world)
        {
            _world = world;
            _world.OnObjectCreated += HandleCreated;
            _world.OnObjectUpdated += HandleUpdated;
            _world.OnObjectDestroyed += HandleDestroyed;
        }

        public void Dispose()
        {
            _world.OnObjectCreated -= HandleCreated;
            _world.OnObjectUpdated -= HandleUpdated;
            _world.OnObjectDestroyed -= HandleDestroyed;
        }

        private void HandleCreated(SimGameObject obj)
        {
            var dto = new CreatedObjectDto
            {
                id = obj.Id,
                master = obj.Master.ToString(),
                position = ToUnityVector(obj.Position),
                type = obj.Type.ToString()
            };
            ObjectSpawner.Instance.SpawnObject(dto);
        }

        private void HandleUpdated(SimGameObject obj)
        {
            ServedObject served = ObjectContainer.Instance.FindById(obj.Id);
            if (served == null) return;

            var dto = new UpdatedObjectDto
            {
                id = obj.Id,
                position = ToUnityVector(obj.Position),
                hp = GetHp(obj),
                maxHp = GetMaxHp(obj),
                status = obj.Status.ToString(),
                effect = obj.Effect.ToString()
            };
            served.UpdateObject(dto);
        }

        private void HandleDestroyed(SimGameObject obj)
        {
            ServedObject served = ObjectContainer.Instance.FindById(obj.Id);
            if (served == null) return;

            var dto = new UpdatedObjectDto
            {
                id = obj.Id,
                position = ToUnityVector(obj.Position),
                hp = 0,
                maxHp = GetMaxHp(obj),
                status = "Destroyed",
                effect = "None"
            };
            served.UpdateObject(dto);
        }

        private static Vector3 ToUnityVector(SimVector3 v)
        {
            return new Vector3(v.X.ToFloat(), v.Y.ToFloat(), v.Z.ToFloat());
        }

        private static int GetHp(SimGameObject obj)
        {
            var mob = obj.GetComponent<SimMob>();
            return mob?.Hp ?? 0;
        }

        private static int GetMaxHp(SimGameObject obj)
        {
            var mob = obj.GetComponent<SimMob>();
            return mob?.MaxHp ?? 0;
        }

        /// <summary>
        /// Update UI elements (mana, HP, cards) after each simulation step.
        /// </summary>
        public void UpdateUI()
        {
            var match = SceneContext.MatchInfo;
            bool isLeft = SceneContext.UserID == match.leftUser.id;
            var myData = isLeft ? _world.LeftPlayerData : _world.RightPlayerData;

            GameScene.GameSceneUIController.Instance.UpdateMana(myData.Mana.ToInt());
        }
    }
}
