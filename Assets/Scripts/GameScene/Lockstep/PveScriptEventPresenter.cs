using System.Collections;
using System.Collections.Generic;
using GameScene.Simulation.Pve;
using GameScene.Simulation.Rendering;
using GameScene.ServedObjectComponent;
using Global;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Lockstep
{
    public sealed class PveScriptEventPresenter : MonoBehaviour
    {
        private SimulationRendererBridge rendererBridge;

        public void Configure(SimulationRendererBridge value) => rendererBridge = value;

        public void Present(PveScriptEvent scriptEvent)
        {
            bool hasLine = false;
            if (scriptEvent.Lines != null)
                for (int index = 0; index < scriptEvent.Lines.Count; index++)
                {
                    string line = scriptEvent.Lines[index];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    PresentLine(scriptEvent.SpeakerEntityId, line);
                    hasLine = true;
                }
            if (!hasLine && !string.IsNullOrWhiteSpace(scriptEvent.MessageKey))
                PresentLine(scriptEvent.SpeakerEntityId, scriptEvent.MessageKey);
        }

        private void PresentLine(int speakerEntityId, string line)
        {
            GameObject speakerView = rendererBridge != null ? rendererBridge.FindView(speakerEntityId) : null;
            ServedObject speaker = speakerView != null ? speakerView.GetComponent<ServedObject>() : null;
            if (speaker != null)
            {
                PveSpeechBubbleUI.Show(speaker, line);
                return;
            }
            SystemMessageUI.Instance?.ShowMessage(line);
        }
    }

    internal sealed class PveSpeechBubbleUI : MonoBehaviour
    {
        private readonly struct BubbleMessage
        {
            public readonly ServedObject Target;
            public readonly string Message;

            public BubbleMessage(ServedObject target, string message)
            {
                Target = target;
                Message = message;
            }
        }

        private static PveSpeechBubbleUI instance;
        private readonly Queue<BubbleMessage> messages = new Queue<BubbleMessage>();
        private RectTransform canvasRect;
        private RectTransform bubbleRoot;
        private RectTransform backgroundRect;
        private RectTransform textRect;
        private TextMeshProUGUI bubbleText;
        private ServedObject activeTarget;
        private Coroutine routine;
        private Camera worldCamera;

        public static void Show(ServedObject target, string message)
        {
            if (target == null || string.IsNullOrWhiteSpace(message)) return;
            Instance.Enqueue(target, message);
        }

        private static PveSpeechBubbleUI Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = new GameObject(nameof(PveSpeechBubbleUI)).AddComponent<PveSpeechBubbleUI>();
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
            if (instance == this) instance = null;
        }

        private void LateUpdate()
        {
            if (activeTarget != null) UpdatePosition();
        }

        private void Enqueue(ServedObject target, string message)
        {
            messages.Enqueue(new BubbleMessage(target, message));
            if (routine == null) routine = StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            while (messages.Count > 0)
            {
                BubbleMessage current = messages.Peek();
                if (current.Target == null)
                {
                    messages.Dequeue();
                    continue;
                }
                activeTarget = current.Target;
                SetMessage(current.Message);
                bubbleRoot.gameObject.SetActive(true);
                UpdatePosition();
                float elapsed = 0f;
                while (elapsed < 3.5f && activeTarget != null)
                {
                    elapsed += Time.deltaTime;
                    if (messages.Count > 1 && elapsed >= 0.75f) break;
                    yield return null;
                }
                messages.Dequeue();
            }
            activeTarget = null;
            bubbleRoot.gameObject.SetActive(false);
            routine = null;
        }

        private void CreateUi()
        {
            GameObject canvasObject = new GameObject("PveSpeechBubbleCanvas");
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2000;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasRect = canvas.GetComponent<RectTransform>();

            bubbleRoot = CreateRect("SpeechBubble", canvasRect);
            bubbleRoot.anchorMin = bubbleRoot.anchorMax = new Vector2(0.5f, 0.5f);
            bubbleRoot.pivot = new Vector2(0.5f, 0f);
            backgroundRect = CreateRect("Background", bubbleRoot);
            backgroundRect.anchorMin = backgroundRect.anchorMax = new Vector2(0.5f, 0f);
            backgroundRect.pivot = new Vector2(0.5f, 0f);
            Image background = backgroundRect.gameObject.AddComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>("UI/SpeechBubble");
            background.sprite = sprite != null ? sprite : Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            background.type = sprite != null ? Image.Type.Simple : Image.Type.Sliced;
            background.color = new Color(1f, 1f, 1f, 0.96f);
            background.raycastTarget = false;

            textRect = CreateRect("Text", backgroundRect);
            textRect.anchorMin = textRect.anchorMax = textRect.pivot = new Vector2(0.5f, 0.5f);
            bubbleText = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            bubbleText.alignment = TextAlignmentOptions.Center;
            bubbleText.enableWordWrapping = true;
            bubbleText.fontSize = 26f;
            bubbleText.color = new Color(0.16f, 0.16f, 0.16f, 1f);
            bubbleText.raycastTarget = false;
            bubbleRoot.gameObject.SetActive(false);
        }

        private void SetMessage(string message)
        {
            bubbleText.text = message;
            Vector2 preferred = bubbleText.GetPreferredValues(message, 320f, 0f);
            preferred.x = Mathf.Min(preferred.x, 320f);
            textRect.sizeDelta = preferred;
            textRect.anchoredPosition = new Vector2(0f, 18f);
            Vector2 size = new Vector2(preferred.x + 108f, preferred.y + 96f);
            backgroundRect.sizeDelta = size;
            bubbleRoot.sizeDelta = size;
        }

        private void UpdatePosition()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (worldCamera == null || activeTarget == null)
            {
                bubbleRoot.gameObject.SetActive(false);
                return;
            }
            Vector3 screen = worldCamera.WorldToScreenPoint(activeTarget.GetSpeechBubbleAnchorWorldPosition());
            if (screen.z <= 0f)
            {
                bubbleRoot.gameObject.SetActive(false);
                return;
            }
            bubbleRoot.gameObject.SetActive(true);
            screen.y += 42f;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 point);
            float halfWidth = canvasRect.rect.width * 0.5f;
            float halfHeight = canvasRect.rect.height * 0.5f;
            point.x = Mathf.Clamp(point.x, -halfWidth + bubbleRoot.sizeDelta.x * 0.5f,
                halfWidth - bubbleRoot.sizeDelta.x * 0.5f);
            point.y = Mathf.Clamp(point.y, -halfHeight, halfHeight - bubbleRoot.sizeDelta.y);
            bubbleRoot.anchoredPosition = point;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }
    }
}
