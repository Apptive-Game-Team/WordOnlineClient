using System;
using DG.Tweening;
using Global;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.ServedObjectComponent
{
    /// <summary>
    /// Swaps WaterSlime to its spit frame while it attacks.
    /// <para>
    /// The prefab already idles on the gather pose, so the two drawn frames are the idle sprite
    /// and the spit sprite. Both are cut from the same concept canvas and are trimmed, with the
    /// spit frame carrying a custom pivot that puts it back on the idle frame's anchor. Only
    /// <see cref="SpriteRenderer.sprite"/> changes, so the facing set by
    /// <see cref="ServedObject.SetMaster"/> and the hopping idle motion are left alone.
    /// </para>
    /// </summary>
    internal sealed class WaterSlimeSpitPresenter : MonoBehaviour
    {
        private const string SupportedType = "WaterSlime";
        private const string SpitSpritePath = "Game/sprites/WaterSlimeAttackSpit";

        // The server creates the WaterShot projection in the same frame it raises the Attack
        // status, so the spit frame shows up with the projectile instead of trailing it.
        private const float SpitDuration = 0.15f;

        private static Sprite spitSprite;
        private static bool spritesLoaded;

        private ServedObject servedObject;
        private SpriteRenderer animatedRenderer;
        private Sequence sequence;
        private Sprite idleSprite;
        private bool configured;
        private bool subscribed;

        public static void Attach(ServedObject servedObject, string objectType)
        {
            if (servedObject == null || !string.Equals(objectType, SupportedType, StringComparison.Ordinal))
            {
                return;
            }

            WaterSlimeSpitPresenter presenter = servedObject.GetComponent<WaterSlimeSpitPresenter>();
            if (presenter == null)
            {
                presenter = servedObject.gameObject.AddComponent<WaterSlimeSpitPresenter>();
            }

            presenter.Configure(servedObject);
        }

        private static void LoadSprites()
        {
            if (spritesLoaded)
            {
                return;
            }

            spritesLoaded = true;
            spitSprite = Resources.Load<Sprite>(SpitSpritePath);
            if (spitSprite == null)
            {
                WDebug.LogWarning($"WaterSlime spit frame missing at Resources/{SpitSpritePath}.");
            }
        }

        private void Configure(ServedObject target)
        {
            if (configured)
            {
                return;
            }

            configured = true;
            servedObject = target;
            Transform actualTransform = servedObject.GetActualTransform();
            animatedRenderer = actualTransform != null
                ? actualTransform.GetComponentInChildren<SpriteRenderer>()
                : null;
            if (animatedRenderer != null)
            {
                idleSprite = animatedRenderer.sprite;
            }

            LoadSprites();
            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopAndRestore();
        }

        private void Subscribe()
        {
            if (!configured || subscribed || servedObject == null)
            {
                return;
            }

            servedObject.OnAttack += Play;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || servedObject == null)
            {
                return;
            }

            servedObject.OnAttack -= Play;
            subscribed = false;
        }

        private void Play()
        {
            if (animatedRenderer == null || spitSprite == null)
            {
                return;
            }

            StopAndRestore();
            idleSprite = animatedRenderer.sprite;
            animatedRenderer.sprite = spitSprite;

            Sequence spitSequence = DOTween.Sequence();
            sequence = spitSequence;
            spitSequence.SetLink(gameObject);
            spitSequence
                .AppendInterval(SpitDuration)
                .AppendCallback(RestoreIdle)
                .OnComplete(() =>
                {
                    if (sequence == spitSequence)
                    {
                        sequence = null;
                    }
                })
                .OnKill(() =>
                {
                    if (sequence == spitSequence)
                    {
                        sequence = null;
                    }

                    RestoreIdle();
                });
        }

        private void StopAndRestore()
        {
            Sequence activeSequence = sequence;
            sequence = null;
            if (activeSequence != null && activeSequence.IsActive())
            {
                activeSequence.Kill(false);
            }

            RestoreIdle();
        }

        private void RestoreIdle()
        {
            if (animatedRenderer != null && idleSprite != null)
            {
                animatedRenderer.sprite = idleSprite;
            }
        }
    }
}
