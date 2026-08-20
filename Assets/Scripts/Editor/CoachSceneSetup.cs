#if UNITY_EDITOR
using GameScene.Coach;
using Global.Coach;
using LobbyScene.Coach;
using LobbyScene.SettingPage;
using TMPro;
using TutorialScene;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WordOnline.EditorTools
{
    /// <summary>
    /// Wires the coach hint system into a scene and into the settings panel.
    /// Both jobs are pure object graph plumbing, so doing them from a menu item
    /// keeps the scene and prefab YAML out of hand-editing range.
    /// </summary>
    public static class CoachSceneSetup
    {
        private const string CoachRootName = "CoachSystem";
        private const string CloseButtonName = "CoachCloseButton";
        private const string ToggleName = "CoachHintToggle";
        private const string FillName = "Fill";
        private const string TitleLabelName = "TitleLabel";
        private const string StateLabelName = "StateLabel";
        private const string SoundSlidersName = "SoundSliders";

        private const string PanelPrefabPath = "Assets/Prefabs/UI/Tutorial/TutorialMessagePanel.prefab";
        private const string SettingsPrefabPath = "Assets/Prefabs/UI/Lobby/Panal.prefab";

        // 아래 값은 전부 .agents/docs/DESIGN.md 의 토큰이다. 그 문서와 같이 움직인다.
        private const string CardSpriteGuid = "aa4924c3b99854f929c4321e387b3cf1";
        private const string PretendardRegularGuid = "9a8c64c89aee44fa2ac41fff91221f41";
        private const float PanelPixelsPerUnitMultiplier = 2f;
        private const float ToggleWidth = 200f;
        private const float ToggleHeight = 50f;
        private const float LabelPadding = 20f;
        private const float LabelFontSize = 25f;
        private const float CloseButtonSize = 44f;
        private const float CloseButtonInset = 8f;

        private static readonly Color BrownBase = new Color32(0xD9, 0x9F, 0x71, 0xFF);
        private static readonly Color Primary = new Color32(0x2F, 0xB8, 0xA8, 0xFF);
        private static readonly Color Secondary = new Color32(0x2D, 0x35, 0x43, 0xFF);
        private static readonly Color TextDark = new Color32(0x00, 0x00, 0x00, 0xFF);
        private static readonly Color TextLight = new Color32(0xD7, 0xDE, 0xE8, 0xFF);

        [MenuItem("Tools/Coach/Setup Coach In Active Scene")]
        public static void SetupActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();

            GameObject root = GameObject.Find(CoachRootName);
            if (root == null)
            {
                root = new GameObject(CoachRootName);
                Undo.RegisterCreatedObjectUndo(root, "Create Coach System");
            }

            CoachDirector director = GetOrAddComponent<CoachDirector>(root);
            CoachHighlighter highlighter = GetOrAddComponent<CoachHighlighter>(root);
            AddProviderForScene(scene.name, root);

            TutorialPanel panel = InstantiatePanel(root);

            Button closeButton = panel != null ? BuildCloseButton(panel.RootRectTransform) : null;

            SerializedObject serialized = new SerializedObject(director);
            serialized.FindProperty("panel").objectReferenceValue = panel;
            serialized.FindProperty("highlighter").objectReferenceValue = highlighter;
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[CoachSceneSetup] Wired the coach system into {scene.name}. Save the scene to keep it.");
        }

        [MenuItem("Tools/Coach/Add Hint Toggle To Settings Panel")]
        public static void AddSettingsToggle()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(SettingsPrefabPath);
            if (contents == null)
            {
                Debug.LogError($"[CoachSceneSetup] Could not open {SettingsPrefabPath}.");
                return;
            }

            try
            {
                Transform parent = FindDeepChild(contents.transform, SoundSlidersName);
                if (parent == null)
                {
                    Debug.LogError($"[CoachSceneSetup] {SoundSlidersName} was not found in the settings prefab.");
                    return;
                }

                if (FindDeepChild(contents.transform, ToggleName) != null)
                {
                    Debug.Log("[CoachSceneSetup] The settings panel already carries the hint toggle.");
                    return;
                }

                GameObject toggleObject = BuildToggle(parent);

                CoachDataSetter setter = contents.GetComponent<CoachDataSetter>();
                if (setter == null)
                {
                    setter = contents.AddComponent<CoachDataSetter>();
                }

                SerializedObject serialized = new SerializedObject(setter);
                serialized.FindProperty("coachToggle").objectReferenceValue = toggleObject.GetComponent<Toggle>();
                serialized.FindProperty("titleLabel").objectReferenceValue =
                    FindDeepChild(toggleObject.transform, TitleLabelName).GetComponent<TMP_Text>();
                serialized.FindProperty("stateLabel").objectReferenceValue =
                    FindDeepChild(toggleObject.transform, StateLabelName).GetComponent<TMP_Text>();
                serialized.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(contents, SettingsPrefabPath);
                Debug.Log("[CoachSceneSetup] Added the hint toggle to the settings panel.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        /// <summary>
        /// Unity's stock toggle is a small checkmark on a white box, which says
        /// almost nothing about its state and matches nothing else in this
        /// project. This builds the switch from the design system instead: the
        /// whole row fills with the primary teal when hints are on, and a word
        /// beside it spells the state out for anyone the colour does not reach.
        /// </summary>
        private static GameObject BuildToggle(Transform parent)
        {
            Sprite panelSprite = LoadByGuid<Sprite>(CardSpriteGuid);
            TMP_FontAsset font = LoadByGuid<TMP_FontAsset>(PretendardRegularGuid);

            GameObject toggleObject = new GameObject(ToggleName, typeof(RectTransform), typeof(Image), typeof(Toggle));
            RectTransform rect = (RectTransform)toggleObject.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(ToggleWidth, ToggleHeight);

            Image background = toggleObject.GetComponent<Image>();
            StyleAsPanel(background, panelSprite, BrownBase);

            GameObject fillObject = new GameObject(FillName, typeof(RectTransform), typeof(Image));
            RectTransform fillRect = (RectTransform)fillObject.transform;
            fillRect.SetParent(rect, false);
            Stretch(fillRect);

            Image fill = fillObject.GetComponent<Image>();
            StyleAsPanel(fill, panelSprite, Primary);

            TMP_Text title = BuildLabel(rect, TitleLabelName, font, TextAlignmentOptions.MidlineLeft);
            title.text = "훈수";

            TMP_Text state = BuildLabel(rect, StateLabelName, font, TextAlignmentOptions.MidlineRight);
            state.text = "켬";

            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = fill;
            toggle.isOn = true;

            Undo.RegisterCreatedObjectUndo(toggleObject, "Create Coach Hint Toggle");
            return toggleObject;
        }

        private static TMP_Text BuildLabel(RectTransform parent, string name, TMP_FontAsset font, TextAlignmentOptions alignment)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = (RectTransform)labelObject.transform;
            rect.SetParent(parent, false);
            Stretch(rect);
            rect.offsetMin = new Vector2(LabelPadding, 0f);
            rect.offsetMax = new Vector2(-LabelPadding, 0f);

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            if (font != null)
            {
                text.font = font;
            }

            text.fontSize = LabelFontSize;
            text.color = TextDark;
            text.alignment = alignment;
            text.enableWordWrapping = false;

            return text;
        }

        private static void StyleAsPanel(Image image, Sprite sprite, Color color)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = PanelPixelsPerUnitMultiplier;
            image.color = color;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static T LoadByGuid<T>(string guid) where T : Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"[CoachSceneSetup] Asset {guid} was not found; the toggle keeps Unity's default look for it.");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// A hint the player cannot get rid of is worse than no hint. This puts
        /// a close button in the panel's top-right corner, parented to the panel
        /// so it comes and goes with it.
        /// </summary>
        private static Button BuildCloseButton(RectTransform panelRect)
        {
            if (panelRect == null)
            {
                Debug.LogWarning("[CoachSceneSetup] The panel has no RectTransform, so no close button was added.");
                return null;
            }

            Transform existing = FindDeepChild(panelRect, CloseButtonName);
            if (existing != null)
            {
                return existing.GetComponent<Button>();
            }

            Sprite panelSprite = LoadByGuid<Sprite>(CardSpriteGuid);
            TMP_FontAsset font = LoadByGuid<TMP_FontAsset>(PretendardRegularGuid);

            GameObject buttonObject = new GameObject(CloseButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = (RectTransform)buttonObject.transform;
            rect.SetParent(panelRect, false);
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.sizeDelta = new Vector2(CloseButtonSize, CloseButtonSize);
            rect.anchoredPosition = new Vector2(-CloseButtonInset, -CloseButtonInset);

            Image background = buttonObject.GetComponent<Image>();
            StyleAsPanel(background, panelSprite, Secondary);

            TMP_Text label = BuildLabel(rect, "Label", font, TextAlignmentOptions.Midline);
            label.text = "X";
            label.color = TextLight;
            ((RectTransform)label.transform).offsetMin = Vector2.zero;
            ((RectTransform)label.transform).offsetMax = Vector2.zero;

            buttonObject.GetComponent<Button>().targetGraphic = background;

            Undo.RegisterCreatedObjectUndo(buttonObject, "Create Coach Close Button");
            return buttonObject.GetComponent<Button>();
        }

        private static void AddProviderForScene(string sceneName, GameObject root)
        {
            if (sceneName == "LobbyScene")
            {
                GetOrAddComponent<LobbyCoachRuleProvider>(root);
                return;
            }

            GetOrAddComponent<GameCoachRuleProvider>(root);
        }

        private static TutorialPanel InstantiatePanel(GameObject root)
        {
            TutorialPanel existing = root.GetComponentInChildren<TutorialPanel>(true);
            if (existing != null)
            {
                return existing;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[CoachSceneSetup] Could not load {PanelPrefabPath}.");
                return null;
            }

            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : root.transform;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = "CoachPanel";
            Undo.RegisterCreatedObjectUndo(instance, "Create Coach Panel");

            // The coach never blocks input, so nothing here dims or gates the screen.
            instance.SetActive(false);

            return instance.GetComponent<TutorialPanel>();
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(target);
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent.name == name)
            {
                return parent;
            }

            for (int index = 0; index < parent.childCount; index++)
            {
                Transform found = FindDeepChild(parent.GetChild(index), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
