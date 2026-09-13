using GameScene.Dto.Projectile;
using UnityEngine;

namespace GameScene.Object.Projectile
{
    /// <summary>
    /// Draws the Spirit Bomb as two energy strands that coil around the live
    /// start/end references. The server emits one short projection per damage
    /// tick, so consecutive projections naturally keep the four-second beam
    /// aimed at its current target.
    /// </summary>
    public class SpiritBombBeamProjectile : MonoBehaviour, IProjectile
    {
        private const int SegmentCount = 25;
        private const float CoilCount = 3f;
        private const float CoilRadius = 0.12f;
        private const float BeamWidth = 0.065f;

        private static readonly Color NatureColor = new Color(0.25f, 1f, 0.3f, 0.95f);
        private static readonly Color LightningColor = new Color(1f, 0.92f, 0.18f, 0.95f);

        private ProjectileTarget startTarget;
        private ProjectileTarget endTarget;
        private LineRenderer natureStrand;
        private LineRenderer lightningStrand;
        private Material beamMaterial;
        private float startedAt;

        public void Init(ProjectileDto projectileDto)
        {
            startTarget = projectileDto.start;
            endTarget = projectileDto.end;
            startedAt = Time.time;

            Shader shader = Shader.Find("Sprites/Default");
            beamMaterial = new Material(shader);
            natureStrand = CreateStrand("NatureStrand", NatureColor, 0);
            lightningStrand = CreateStrand("LightningStrand", LightningColor, 1);
            UpdateStrands();
        }

        private void LateUpdate()
        {
            if (startTarget == null || endTarget == null)
            {
                return;
            }

            UpdateStrands();
        }

        private void OnDestroy()
        {
            if (beamMaterial != null)
            {
                Destroy(beamMaterial);
            }
        }

        private LineRenderer CreateStrand(string strandName, Color color, int sortingOrder)
        {
            GameObject strandObject = new GameObject(strandName);
            strandObject.transform.SetParent(transform, false);

            LineRenderer strand = strandObject.AddComponent<LineRenderer>();
            strand.useWorldSpace = true;
            strand.positionCount = SegmentCount;
            strand.widthMultiplier = BeamWidth;
            strand.numCapVertices = 3;
            strand.numCornerVertices = 2;
            strand.textureMode = LineTextureMode.Stretch;
            strand.material = beamMaterial;
            strand.startColor = color;
            strand.endColor = new Color(color.r, color.g, color.b, 0.45f);
            strand.sortingLayerName = "Default";
            strand.sortingOrder = sortingOrder;
            return strand;
        }

        private void UpdateStrands()
        {
            Vector3 start = ProjectileUtil.GetPosition(startTarget);
            Vector3 end = ProjectileUtil.GetPosition(endTarget);
            Vector3 screenUp = ProjectileUtil.GetScreenUp();
            float animatedPhase = (Time.time - startedAt) * Mathf.PI * 4f;

            for (int index = 0; index < SegmentCount; index++)
            {
                float progress = index / (SegmentCount - 1f);
                float taper = Mathf.Sin(progress * Mathf.PI);
                float phase = progress * Mathf.PI * 2f * CoilCount - animatedPhase;
                Vector3 offset = screenUp * (Mathf.Sin(phase) * CoilRadius * taper);
                Vector3 center = Vector3.Lerp(start, end, progress);

                natureStrand.SetPosition(index, center + offset);
                lightningStrand.SetPosition(index, center - offset);
            }
        }
    }
}
