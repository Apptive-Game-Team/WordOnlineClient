using UnityEngine;

namespace GameScene.Dto.debug
{
    [System.Serializable]
    public class Gizmo
    {
        private Vector3 relativePosition;
        private float radius;
        private Vector3 boxSize;
        private string type; // Circle, Box
        private string category; // Collider, AttackRange, AreaOfEffect, DetectionRange, SpawnArea
    }
}