using System.Collections;
using System.Collections.Generic;
using GameScene.Dto;
using GameScene.Object;
using GameScene.ServedObjectComponent;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Handler
{
    public class PveScriptEventHandler : IFrameInfoHandler<PveScriptEventInfo>
    {
        public void Handler(PveScriptEventInfo pveScriptEvent)
        {
            if (pveScriptEvent == null)
            {
                return;
            }

            if (pveScriptEvent.lines != null && pveScriptEvent.lines.Count > 0)
            {
                foreach (string line in pveScriptEvent.lines)
                {
                    PveDialoguePresenter.ShowLine(pveScriptEvent.speakerObjectId, line);
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(pveScriptEvent.key))
            {
                PveDialoguePresenter.ShowLine(pveScriptEvent.speakerObjectId, pveScriptEvent.key);
            }
        }
    }

    internal static class PveDialoguePresenter
    {
        public static void ShowLine(int speakerObjectId, string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            if (PveSpeechBubbleUI.TryShowMessage(speakerObjectId, line))
            {
                return;
            }

            SystemMessageUI.Instance.ShowMessage(line);
        }
    }

    internal class PveSpeechBubbleUI : MonoBehaviour
    {
        private struct BubbleMessage
        {
            public ServedObject target;
            public string message;
        }

        private static PveSpeechBubbleUI instance;

        [SerializeField] private float duration = 3.5f;
        [SerializeField] private float replaceDelay = 0.75f;
        [SerializeField] private float maxTextWidth = 320f;
        [SerializeField] private float horizontalPadding = 54f;
        [SerializeField] private float verticalPadding = 48f;
        [SerializeField] private float screenOffsetY = 42f;

        private readonly Queue<BubbleMessage> messageQueue = new Queue<BubbleMessage>();

        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform bubbleRoot;
        private RectTransform backgroundRect;
        private RectTransform textRect;
        private TextMeshProUGUI bubbleText;
        private Coroutine messageRoutine;
        private ServedObject activeTarget;
        private Camera worldCamera;
        private Image backgroundImage;
        private Sprite bubbleSprite;

        public static bool TryShowMessage(int speakerObjectId, string message)
        {
            if (speakerObjectId <= 0 || string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            if (ObjectContainer.Instance == null)
            {
                return false;
            }

            ServedObject target = ObjectContainer.Instance.FindById(speakerObjectId);
            if (target == null)
            {
                return false;
            }

            Instance.Enqueue(target, message);
            return true;
        }

        private static PveSpeechBubbleUI Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject root = new GameObject(nameof(PveSpeechBubbleUI));
                    instance = root.AddComponent<PveSpeechBubbleUI>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            CreateUi();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void LateUpdate()
        {
            if (activeTarget == null)
            {
                return;
            }

            UpdateBubblePosition();
        }

        private void Enqueue(ServedObject target, string message)
        {
            messageQueue.Enqueue(new BubbleMessage
            {
                target = target,
                message = message
            });

            if (messageRoutine == null)
            {
                messageRoutine = StartCoroutine(ProcessMessageQueue());
            }
        }

        private IEnumerator ProcessMessageQueue()
        {
            while (messageQueue.Count > 0)
            {
                BubbleMessage current = messageQueue.Peek();
                if (current.target == null)
                {
                    messageQueue.Dequeue();
                    continue;
                }

                activeTarget = current.target;
                SetBubbleMessage(current.message);
                bubbleRoot.gameObject.SetActive(true);
                UpdateBubblePosition();

                float timer = 0f;
                while (timer < duration)
                {
                    if (activeTarget == null)
                    {
                        break;
                    }

                    timer += Time.deltaTime;
                    if (messageQueue.Count > 1 && timer >= replaceDelay)
                    {
                        break;
                    }

                    yield return null;
                }

                messageQueue.Dequeue();
            }

            activeTarget = null;
            bubbleRoot.gameObject.SetActive(false);
            messageRoutine = null;
        }

        private void CreateUi()
        {
            GameObject canvasObject = new GameObject("SpeechBubbleCanvas");
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            canvasRect = canvas.GetComponent<RectTransform>();

            bubbleRoot = CreateRect("SpeechBubble", canvasRect);
            bubbleRoot.anchorMin = new Vector2(0.5f, 0.5f);
            bubbleRoot.anchorMax = new Vector2(0.5f, 0.5f);
            bubbleRoot.pivot = new Vector2(0.5f, 0f);

            backgroundRect = CreateRect("Background", bubbleRoot);
            backgroundRect.anchorMin = new Vector2(0.5f, 0f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0f);
            backgroundRect.pivot = new Vector2(0.5f, 0f);

            backgroundImage = backgroundRect.gameObject.AddComponent<Image>();
            bubbleSprite = Resources.Load<Sprite>("UI/SpeechBubble");
            backgroundImage.sprite = bubbleSprite != null
                ? bubbleSprite
                : Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            backgroundImage.type = bubbleSprite != null ? Image.Type.Simple : Image.Type.Sliced;
            backgroundImage.color = new Color(1f, 1f, 1f, 0.96f);
            backgroundImage.raycastTarget = false;

            textRect = CreateRect("Text", backgroundRect);
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);

            bubbleText = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            bubbleText.alignment = TextAlignmentOptions.Center;
            bubbleText.enableWordWrapping = true;
            bubbleText.fontSize = 26f;
            bubbleText.color = new Color(0.16f, 0.16f, 0.16f, 1f);
            bubbleText.raycastTarget = false;

            if (bubbleSprite == null)
            {
                Outline backgroundOutline = backgroundRect.gameObject.AddComponent<Outline>();
                backgroundOutline.effectColor = new Color(0f, 0f, 0f, 0.18f);
                backgroundOutline.effectDistance = new Vector2(2f, -2f);
            }

            bubbleRoot.gameObject.SetActive(false);
        }

        private void SetBubbleMessage(string message)
        {
            bubbleText.text = message;

            Vector2 preferredSize = bubbleText.GetPreferredValues(message, maxTextWidth, 0f);
            preferredSize.x = Mathf.Min(preferredSize.x, maxTextWidth);

            textRect.sizeDelta = preferredSize;

            Vector2 backgroundSize = new Vector2(
                preferredSize.x + horizontalPadding * 2f,
                preferredSize.y + verticalPadding * 2f
            );
            backgroundRect.sizeDelta = backgroundSize;
            backgroundRect.anchoredPosition = bubbleSprite != null ? Vector2.zero : new Vector2(0f, 10f);

            textRect.anchoredPosition = bubbleSprite != null ? new Vector2(0f, 18f) : Vector2.zero;

            if (backgroundImage != null && bubbleSprite != null)
            {
                backgroundImage.preserveAspect = false;
            }

            bubbleRoot.sizeDelta = backgroundSize;
        }

        private void UpdateBubblePosition()
        {
            EnsureWorldCamera();
            if (worldCamera == null || activeTarget == null)
            {
                bubbleRoot.gameObject.SetActive(false);
                return;
            }

            Vector3 screenPoint = worldCamera.WorldToScreenPoint(activeTarget.GetSpeechBubbleAnchorWorldPosition());
            if (screenPoint.z <= 0f)
            {
                bubbleRoot.gameObject.SetActive(false);
                return;
            }

            bubbleRoot.gameObject.SetActive(true);
            screenPoint.y += screenOffsetY;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);

            float halfCanvasWidth = canvasRect.rect.width * 0.5f;
            float halfCanvasHeight = canvasRect.rect.height * 0.5f;
            float halfBubbleWidth = bubbleRoot.sizeDelta.x * 0.5f;
            float bubbleHeight = bubbleRoot.sizeDelta.y;

            localPoint.x = Mathf.Clamp(localPoint.x, -halfCanvasWidth + halfBubbleWidth, halfCanvasWidth - halfBubbleWidth);
            localPoint.y = Mathf.Clamp(localPoint.y, -halfCanvasHeight, halfCanvasHeight - bubbleHeight);

            bubbleRoot.anchoredPosition = localPoint;
        }

        private void EnsureWorldCamera()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject child = new GameObject(objectName, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }
    }
}
