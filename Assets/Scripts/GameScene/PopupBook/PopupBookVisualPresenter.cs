using DG.Tweening;
using GameScene.ServedObjectComponent;
using GameScene.ServedObjectComponent.Effect;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

namespace GameScene.PopupBook
{
    public class PopupBookVisualPresenter : MonoBehaviour
    {
        private const float SpawnStartAngle = -84f;
        private const float SpawnDuration = 0.42f;

        private Transform visualRoot;
        private Transform spawnPresentationPivot;
        private Camera worldCamera;
        private ServedObjectWorldUI worldUi;
        private bool worldUiEnabledState;
        private bool hasWorldUiEnabledState;
        private Sequence spawnSequence;

        public static PopupBookVisualPresenter Attach(ServedObject servedObject)
        {
            Transform actualTransform = servedObject.GetActualTransform();
            if (actualTransform == servedObject.transform || actualTransform.parent == null)
            {
                return null;
            }

            PopupBookVisualPresenter existingPresenter =
                actualTransform.GetComponentInParent<PopupBookVisualPresenter>();
            if (existingPresenter != null)
            {
                return existingPresenter;
            }

            Transform originalParent = actualTransform.parent;
            Vector3 originalLocalPosition = actualTransform.localPosition;
            Quaternion originalLocalRotation = actualTransform.localRotation;
            Vector3 originalLocalScale = actualTransform.localScale;

            GameObject presenterObject = new GameObject(nameof(PopupBookVisualPresenter));
            Transform presenterTransform = presenterObject.transform;
            presenterTransform.SetParent(originalParent, false);
            presenterTransform.localPosition = originalLocalPosition;
            presenterTransform.localRotation = Quaternion.identity;
            presenterTransform.localScale = Vector3.one;

            GameObject pivotObject = new GameObject("SpawnPresentationPivot");
            Transform pivotTransform = pivotObject.transform;
            pivotTransform.SetParent(presenterTransform, false);

            float bottomOffset = GetBottomOffset(actualTransform, originalLocalScale);
            pivotTransform.localPosition = new Vector3(0f, bottomOffset, 0f);

            actualTransform.SetParent(pivotTransform, false);
            actualTransform.localPosition = new Vector3(0f, -bottomOffset, 0f);
            actualTransform.localRotation = originalLocalRotation;
            actualTransform.localScale = originalLocalScale;

            PopupBookVisualPresenter presenter = presenterObject.AddComponent<PopupBookVisualPresenter>();
            presenter.visualRoot = presenterTransform;
            presenter.spawnPresentationPivot = pivotTransform;
            presenter.worldCamera = Camera.main;
            presenter.worldUi = servedObject.GetComponentInChildren<ServedObjectWorldUI>(true);
            presenter.AlignWithCamera();
            presenter.worldUi?.Initialize();
            return presenter;
        }

        public void PlaySpawnPresentation(bool spawnDust)
        {
            if (spawnPresentationPivot == null)
            {
                return;
            }

            StopSpawnPresentation();
            CaptureAndSetWorldSpaceUiEnabled(false);

            spawnPresentationPivot.localRotation = Quaternion.Euler(SpawnStartAngle, 0f, 0f);

            Sequence currentSequence = DOTween.Sequence();
            spawnSequence = currentSequence;
            currentSequence.SetLink(gameObject);
            currentSequence.Append(
                spawnPresentationPivot
                    .DOLocalRotate(Vector3.zero, SpawnDuration, RotateMode.Fast)
                    .SetEase(Ease.OutBack, 1.25f));

            if (spawnDust)
            {
                currentSequence.InsertCallback(SpawnDuration * 0.72f, SpawnBuildingDust);
            }

            currentSequence.OnComplete(() => FinishSpawnPresentation(currentSequence));
            currentSequence.OnKill(() => FinishSpawnPresentation(currentSequence));
        }

        private void OnDestroy()
        {
            StopSpawnPresentation();
        }

        private void AlignWithCamera()
        {
            if (worldCamera == null || visualRoot == null)
            {
                return;
            }

            visualRoot.rotation = worldCamera.transform.rotation;
        }

        private void SpawnBuildingDust()
        {
            if (spawnPresentationPivot == null)
            {
                return;
            }

            SpriteRenderer referenceRenderer = GetComponentInChildren<SpriteRenderer>();
            BuildingSpawnDustEffect.Spawn(
                spawnPresentationPivot.position,
                worldCamera != null ? worldCamera.transform.rotation : Quaternion.identity,
                referenceRenderer);
        }

        private void StopSpawnPresentation()
        {
            Sequence sequenceToKill = spawnSequence;
            spawnSequence = null;
            sequenceToKill?.Kill();

            if (spawnPresentationPivot != null)
            {
                spawnPresentationPivot.localRotation = Quaternion.identity;
            }

            RestoreWorldSpaceUiEnabled();
        }

        private void FinishSpawnPresentation(Sequence completedSequence)
        {
            if (spawnSequence != completedSequence)
            {
                return;
            }

            spawnSequence = null;
            if (spawnPresentationPivot != null)
            {
                spawnPresentationPivot.localRotation = Quaternion.identity;
            }

            RestoreWorldSpaceUiEnabled();
        }

        private void CaptureAndSetWorldSpaceUiEnabled(bool enabled)
        {
            if (worldUi == null)
            {
                return;
            }

            worldUiEnabledState = worldUi.CanvasEnabled;
            hasWorldUiEnabledState = true;
            worldUi.SetCanvasEnabled(enabled);
        }

        private void RestoreWorldSpaceUiEnabled()
        {
            if (worldUi == null || !hasWorldUiEnabledState)
            {
                return;
            }

            worldUi.SetCanvasEnabled(worldUiEnabledState);
            hasWorldUiEnabledState = false;
        }

        private static float GetBottomOffset(Transform actualTransform, Vector3 originalLocalScale)
        {
            SpriteRenderer spriteRenderer = actualTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return 0f;
            }

            return spriteRenderer.sprite.bounds.min.y * Mathf.Abs(originalLocalScale.y);
        }

    }
}
