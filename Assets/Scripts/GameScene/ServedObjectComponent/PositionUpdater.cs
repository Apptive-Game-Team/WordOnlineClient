using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Scripts.Data;
using UnityEngine;

namespace Scripts.GameScene.ServedObjectComponent
{
    public class PositionUpdater
    {
        private readonly Transform transform;
        private readonly SpriteRenderer spriteRenderer;
        
        private TweenerCore<Vector3, Vector3, VectorOptions> moveTween;
        private Vector3? nextPosition = null;
        private double lastX;
        
        public PositionUpdater(Transform transform, SpriteRenderer spriteRenderer)
        {
            this.transform = transform;
            this.spriteRenderer = spriteRenderer;
            lastX = this.transform.position.x;
        }

        internal void UpdatePosition(UpdatedObjectDto updatedObjectDto)
        {
            if (moveTween != null && moveTween.IsActive())
            {
                moveTween.Kill();
            }
            if (nextPosition.HasValue)
            {
                transform.position = nextPosition.Value;
            }
            nextPosition = new Vector3(
                updatedObjectDto.position.x, 
                updatedObjectDto.position.y, 
                updatedObjectDto.position.z);
            moveTween = transform.DOMove(nextPosition.Value, GameConfig.FRAME_DURATION).SetEase(Ease.Linear);
            
            UpdateFlip();
        }

        private void UpdateFlip()
        {
            if (nextPosition?.x == null)
            {
                return;
            }

            double x = (double) nextPosition?.x;

            if (lastX < x)
            {
                spriteRenderer.flipX = false;
            }
            else if (x < lastX)
            {
                spriteRenderer.flipX = true;
            }

            lastX = x;
        }
    }
}