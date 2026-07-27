using System;
using System.Collections.Generic;
using Data;
using Data.Sound;
using DG.Tweening;
using GameScene.Dto;
using GameScene.Exception;
using GameScene.PopupBook;
using GameScene.ServedObjectComponent;
using Global;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.Object
{
    public class ObjectSpawner : LocalSingletonObject<ObjectSpawner>
    {
        
        public void SpawnObject(CreatedObjectDto createdObjectDto, bool playSpawnPresentation = true)
        {
            if (ObjectContainer.Instance.IsExist(createdObjectDto.id))
            {
                WDebug.LogWarning($"Object with ID {createdObjectDto.id} already exists. Spawn aborted.");
                return;
            }
            
            WDebug.Log($"Spawning object: {createdObjectDto.type}, id: {createdObjectDto.id}");
           
            GameObject spawnedObject = InstantiateGameObject(createdObjectDto);
            
            ServedObject servedObject = spawnedObject.GetOrAddComponent<ServedObject>();
            AquaArcherAttackPresenter.Attach(servedObject, createdObjectDto.type);
            AerialLightningDeathPresenter.Attach(servedObject, createdObjectDto.type);
            PopupBookVisualPresenter popupBookPresenter = PopupBookVisualPresenter.Attach(servedObject);
            
            SetAudioSourceVolume(spawnedObject);
            
            servedObject.SetMaster(createdObjectDto.master);
            servedObject.id = createdObjectDto.id;
#if UNITY_EDITOR
            servedObject.SetGizmos(createdObjectDto.gizmos);
#endif
            
            WDebug.Log($"Spawned object: {spawnedObject}, master set to: {createdObjectDto.master}, id set to: {createdObjectDto.id}");
            try
            {
                ObjectContainer.Instance.RegisterObject(servedObject);
                if (playSpawnPresentation && popupBookPresenter != null)
                {
                    popupBookPresenter.PlaySpawnPresentation(
                        SpawnPresentationTypeCatalog.IsBuilding(createdObjectDto.type));
                }
            } catch (DuplicatedException e)
            {
                WDebug.LogError($"Failed to register object: {e.Message}");
                Destroy(spawnedObject);
            }
        }
        
        private void SetAudioSourceVolume(GameObject obj)
        {
            AudioSource[] audioSources = obj.GetComponentsInChildren<AudioSource>();
            foreach (var source in audioSources)
            {
                source.volume = source.volume * SoundData.gameVolume / 100f;
            }
            WDebug.Log($"Spawned object: {obj}, audio sources set: {audioSources.Length}");
        }

        private GameObject InstantiateGameObject(CreatedObjectDto createdObjectDto)
        {
            GameObject spawnedObject;
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{createdObjectDto.type}");
            
            WDebug.Log($"Spawning object: {createdObjectDto.type}, prefab found: {prefab != null}");
            
            if (!prefab)
            {
                spawnedObject = new GameObject(createdObjectDto.type);
                spawnedObject.transform.position = createdObjectDto.position;
            }
            else 
            {
                spawnedObject = Instantiate(prefab, createdObjectDto.position, prefab.transform.rotation);
            }
            
            WDebug.Log($"Spawned object: {spawnedObject}, gameObject created at position {createdObjectDto.position}");
            return spawnedObject;
        }
    }

    internal sealed class AquaArcherAttackPresenter : MonoBehaviour
    {
        private const string SupportedType = "AquaArcher";
        private const float FrameDuration = 0.08f;

        private static readonly Vector3 AnticipationScale = new Vector3(0.9f, 1.08f, 1f);
        private static readonly Vector3 ReleaseScale = new Vector3(1.1f, 0.92f, 1f);

        private ServedObject servedObject;
        private Transform animatedTransform;
        private Sequence sequence;
        private Vector3 idleScale;
        private bool configured;
        private bool subscribed;

        public static void Attach(ServedObject servedObject, string objectType)
        {
            if (servedObject == null || !string.Equals(objectType, SupportedType, StringComparison.Ordinal))
            {
                return;
            }

            AquaArcherAttackPresenter presenter = servedObject.GetComponent<AquaArcherAttackPresenter>();
            if (presenter == null)
            {
                presenter = servedObject.gameObject.AddComponent<AquaArcherAttackPresenter>();
            }

            presenter.Configure(servedObject);
        }

        private void Configure(ServedObject target)
        {
            if (configured)
            {
                return;
            }

            configured = true;
            servedObject = target;
            animatedTransform = servedObject.GetActualTransform();
            idleScale = animatedTransform.localScale;
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
            if (animatedTransform == null)
            {
                return;
            }

            StopAndRestore();
            idleScale = animatedTransform.localScale;
            animatedTransform.localScale = Vector3.Scale(idleScale, AnticipationScale);

            Sequence attackSequence = DOTween.Sequence();
            sequence = attackSequence;
            attackSequence.SetLink(gameObject);
            attackSequence
                .AppendInterval(FrameDuration)
                .AppendCallback(() =>
                {
                    if (animatedTransform != null)
                    {
                        animatedTransform.localScale = Vector3.Scale(idleScale, ReleaseScale);
                    }
                })
                .AppendInterval(FrameDuration)
                .AppendCallback(RestoreIdle)
                .OnComplete(() =>
                {
                    if (sequence == attackSequence)
                    {
                        sequence = null;
                    }
                })
                .OnKill(() =>
                {
                    if (sequence == attackSequence)
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
            if (animatedTransform != null)
            {
                animatedTransform.localScale = idleScale;
            }
        }
    }

    internal sealed class AerialLightningDeathPresenter
    {
        private const string CrackleResourcePath = "Prefabs/Effects/LightningAttackAura";
        private const float FallDistance = 0.8f;
        private const float FallDuration = 0.45f;
        private const float FallRotation = 80f;
        private const float FadeDuration = 0.15f;
        private const float CrackleFallbackLifetime = 1f;

        private static readonly HashSet<string> SupportedTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            "ThunderSpirit",
            "ThunderBird"
        };

        private readonly ServedObject servedObject;
        private bool played;

        private AerialLightningDeathPresenter(ServedObject servedObject)
        {
            this.servedObject = servedObject;
            servedObject.OnDestroyed += Play;
        }

        public static void Attach(ServedObject servedObject, string objectType)
        {
            if (servedObject == null || !SupportedTypes.Contains(objectType))
            {
                return;
            }

            _ = new AerialLightningDeathPresenter(servedObject);
        }

        private void Play()
        {
            if (played)
            {
                return;
            }

            played = true;
            servedObject.OnDestroyed -= Play;

            Transform actualTransform = servedObject.GetActualTransform();
            SpriteRenderer sourceRenderer = actualTransform.GetComponentInChildren<SpriteRenderer>();
            if (sourceRenderer == null || sourceRenderer.sprite == null)
            {
                return;
            }

            GameObject visual = new GameObject($"{servedObject.name}DeathVisual");
            visual.transform.SetPositionAndRotation(sourceRenderer.transform.position, sourceRenderer.transform.rotation);
            visual.transform.localScale = sourceRenderer.transform.lossyScale;

            SpriteRenderer visualRenderer = visual.AddComponent<SpriteRenderer>();
            CopyRendererState(sourceRenderer, visualRenderer);
            PlaySequence(visual, visualRenderer);
        }

        private static void CopyRendererState(SpriteRenderer source, SpriteRenderer destination)
        {
            destination.sprite = source.sprite;
            destination.color = source.color;
            destination.flipX = source.flipX;
            destination.flipY = source.flipY;
            destination.sharedMaterial = source.sharedMaterial;
            destination.sortingLayerID = source.sortingLayerID;
            destination.sortingOrder = source.sortingOrder;
            destination.maskInteraction = source.maskInteraction;
            destination.spriteSortPoint = source.spriteSortPoint;
        }

        private static void PlaySequence(GameObject visual, SpriteRenderer visualRenderer)
        {
            float rotationDirection = visualRenderer.flipX ? 1f : -1f;
            Vector3 landingPosition = visual.transform.position + Vector3.down * FallDistance;
            Vector3 landingRotation = visual.transform.eulerAngles
                                      + Vector3.forward * (FallRotation * rotationDirection);

            Sequence sequence = DOTween.Sequence();
            sequence.SetLink(visual);
            sequence
                .Append(visual.transform.DOMove(landingPosition, FallDuration).SetEase(Ease.InQuad))
                .Join(visual.transform.DORotate(landingRotation, FallDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.InQuad))
                .AppendCallback(() => SpawnCrackle(visual.transform.position, visualRenderer))
                .Append(visualRenderer.DOFade(0f, FadeDuration).SetEase(Ease.OutQuad))
                .OnComplete(() => UnityEngine.Object.Destroy(visual))
                .OnKill(() =>
                {
                    if (visual != null)
                    {
                        UnityEngine.Object.Destroy(visual);
                    }
                });
        }

        private static void SpawnCrackle(Vector3 position, SpriteRenderer referenceRenderer)
        {
            GameObject cracklePrefab = Resources.Load<GameObject>(CrackleResourcePath);
            if (cracklePrefab == null)
            {
                WDebug.LogWarning($"Aerial death crackle prefab not found at Resources/{CrackleResourcePath}.");
                return;
            }

            GameObject crackle = UnityEngine.Object.Instantiate(cracklePrefab, position, Quaternion.identity);
            float visualSize = Mathf.Max(referenceRenderer.bounds.size.x, referenceRenderer.bounds.size.y);
            crackle.transform.localScale = Vector3.one * Mathf.Max(visualSize, 0.1f);

            SpriteRenderer crackleRenderer = crackle.GetComponentInChildren<SpriteRenderer>();
            if (crackleRenderer != null)
            {
                crackleRenderer.sortingLayerID = referenceRenderer.sortingLayerID;
                crackleRenderer.sortingOrder = referenceRenderer.sortingOrder + 1;
            }

            UnityEngine.Object.Destroy(crackle, CrackleFallbackLifetime);
        }
    }
}
