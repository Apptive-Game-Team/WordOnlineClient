using GameScene.ServedObjectComponent;
using UnityEngine;

namespace GameScene.PopupBook
{
    public class PopupBookVisualPresenter : MonoBehaviour
    {
        private Transform visualRoot;
        private Camera worldCamera;
        private Transform[] worldSpaceUiRoots;

        public static void Attach(ServedObject servedObject)
        {
            Transform actualTransform = servedObject.GetActualTransform();
            if (actualTransform == servedObject.transform ||
                actualTransform.parent == null ||
                actualTransform.parent.name == nameof(PopupBookVisualPresenter))
            {
                return;
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

            actualTransform.SetParent(presenterTransform, false);
            actualTransform.localPosition = Vector3.zero;
            actualTransform.localRotation = originalLocalRotation;
            actualTransform.localScale = originalLocalScale;

            PopupBookVisualPresenter presenter = presenterObject.AddComponent<PopupBookVisualPresenter>();
            presenter.visualRoot = presenterTransform;
            presenter.worldCamera = Camera.main;
            presenter.worldSpaceUiRoots = GetWorldSpaceUiRoots(servedObject);
        }

        private void LateUpdate()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (worldCamera != null)
            {
                visualRoot.rotation = worldCamera.transform.rotation;

                if (worldSpaceUiRoots == null)
                {
                    return;
                }

                foreach (Transform uiRoot in worldSpaceUiRoots)
                {
                    if (uiRoot != null)
                    {
                        uiRoot.rotation = worldCamera.transform.rotation;
                    }
                }
            }
        }

        private static Transform[] GetWorldSpaceUiRoots(ServedObject servedObject)
        {
            Canvas[] canvases = servedObject.GetComponentsInChildren<Canvas>(true);
            int worldSpaceCanvasCount = 0;
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    worldSpaceCanvasCount++;
                }
            }

            Transform[] roots = new Transform[worldSpaceCanvasCount];
            int index = 0;
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.WorldSpace)
                {
                    roots[index++] = canvas.transform;
                }
            }

            return roots;
        }
    }
}
