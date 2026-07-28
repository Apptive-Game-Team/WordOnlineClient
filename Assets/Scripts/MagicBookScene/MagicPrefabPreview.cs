using Data.Magic;
using GameScene.Object;
using GameScene.PopupBook;
using GameScene.ServedObjectComponent;
using GameScene.ServedObjectComponent.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace MagicBookScene
{
    public sealed class MagicPrefabPreview : MonoBehaviour
    {
        private const int PreviewLayer = 31;
        private const int TextureSize = 512;
        private const float BoundsPadding = 1.25f;
        private static readonly Vector3 PreviewOrigin = new Vector3(10000f, 10000f, 0f);

        private Image fallbackImage;
        private RawImage previewImage;
        private Camera previewCamera;
        private RenderTexture renderTexture;
        private GameObject previewObject;
        private ServedObject servedObject;

        public static MagicPrefabPreview Attach(Image targetImage)
        {
            if (targetImage == null)
            {
                return null;
            }

            MagicPrefabPreview preview = targetImage.GetComponent<MagicPrefabPreview>();
            if (preview == null)
            {
                preview = targetImage.gameObject.AddComponent<MagicPrefabPreview>();
            }

            preview.Initialize(targetImage);
            return preview;
        }

        public bool Show(CombinedMagicData data)
        {
            ClearPreview();

            if (data == null || string.IsNullOrWhiteSpace(data.resourceName))
            {
                return false;
            }

            string prefabName = data.resourceName;
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
            if (prefab == null || prefab.GetComponentInChildren<SpriteRenderer>(true) == null)
            {
                return false;
            }

            EnsureRenderSurface();
            previewObject = Instantiate(prefab);
            previewObject.name = $"{prefab.name} (Magic Book Preview)";
            SetLayerRecursively(previewObject.transform, PreviewLayer);
            DisableWorldSpaceUi(previewObject);

            servedObject = previewObject.GetComponent<ServedObject>();
            if (servedObject == null)
            {
                servedObject = previewObject.AddComponent<ServedObject>();
            }

            AquaArcherAttackPresenter.Attach(servedObject, prefabName);
            MagmaSpiritSpawnPresenter.Attach(servedObject, prefabName, true);
            ServedObjectSfxController.Attach(servedObject, prefabName, true);

            PopupBookVisualPresenter popupPresenter = PopupBookVisualPresenter.Attach(servedObject);
            popupPresenter?.PlaySpawnPresentation(
                SpawnPresentationTypeCatalog.IsBuilding(prefabName));

            FitCamera();
            fallbackImage.enabled = false;
            previewImage.enabled = true;
            return true;
        }

        public void PlayAttack()
        {
            servedObject?.PlayAttackPresentation();
        }

        private void Initialize(Image targetImage)
        {
            fallbackImage = targetImage;
            if (previewImage != null)
            {
                return;
            }

            GameObject imageObject = new GameObject(
                "PrefabPreview",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage),
                typeof(Button));
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.SetParent(targetImage.rectTransform.parent, false);
            rectTransform.anchorMin = targetImage.rectTransform.anchorMin;
            rectTransform.anchorMax = targetImage.rectTransform.anchorMax;
            rectTransform.anchoredPosition = targetImage.rectTransform.anchoredPosition;
            rectTransform.sizeDelta = targetImage.rectTransform.sizeDelta;
            rectTransform.pivot = targetImage.rectTransform.pivot;
            rectTransform.SetSiblingIndex(targetImage.rectTransform.GetSiblingIndex() + 1);

            previewImage = imageObject.GetComponent<RawImage>();
            previewImage.color = Color.white;
            previewImage.raycastTarget = true;
            previewImage.enabled = false;

            Button button = imageObject.GetComponent<Button>();
            button.targetGraphic = previewImage;
            button.transition = UnityEngine.UI.Selectable.Transition.None;
            button.onClick.AddListener(PlayAttack);
        }

        private void EnsureRenderSurface()
        {
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(TextureSize, TextureSize, 16, RenderTextureFormat.ARGB32)
                {
                    name = "MagicBookPrefabPreview",
                    antiAliasing = 2,
                    filterMode = FilterMode.Bilinear,
                };
                renderTexture.Create();
                previewImage.texture = renderTexture;
            }

            if (previewCamera != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("MagicBookPrefabPreviewCamera");
            cameraObject.transform.SetParent(transform, false);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.cullingMask = 1 << PreviewLayer;
            previewCamera.orthographic = true;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 100f;
            previewCamera.targetTexture = renderTexture;
        }

        private void FitCamera()
        {
            SpriteRenderer[] renderers = previewObject.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            previewObject.transform.position += PreviewOrigin - bounds.center;
            float halfHeight = Mathf.Max(bounds.extents.y, bounds.extents.x);
            previewCamera.orthographicSize = Mathf.Max(0.5f, halfHeight * BoundsPadding);
            previewCamera.transform.position = PreviewOrigin + Vector3.back * 10f;
        }

        private void ClearPreview()
        {
            servedObject = null;
            if (previewObject != null)
            {
                previewObject.SetActive(false);
                Destroy(previewObject);
                previewObject = null;
            }

            if (fallbackImage != null)
            {
                fallbackImage.enabled = true;
            }

            if (previewImage != null)
            {
                previewImage.enabled = false;
            }
        }

        private static void DisableWorldSpaceUi(GameObject root)
        {
            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                canvas.enabled = false;
            }
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
            {
                SetLayerRecursively(child, layer);
            }
        }

        private void OnDestroy()
        {
            ClearPreview();
            if (previewCamera != null)
            {
                Destroy(previewCamera.gameObject);
            }

            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}
