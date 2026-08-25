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
    /// 훈수 시스템을 씬과 설정 패널에 배선한다. 둘 다 오브젝트 그래프를 잇는 단순 작업이라,
    /// 메뉴에서 처리하면 씬과 프리팹 YAML을 손으로 고칠 일이 없어진다.
    /// </summary>
    public static class CoachSceneSetup
    {
        private const string CoachRootName = "CoachSystem";
        private const string CoachPanelName = "CoachPanel";
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
                Debug.Log($"[CoachSceneSetup] {CoachRootName} 을 새로 만들었다.");
            }

            CoachDirector director = GetOrAddComponent<CoachDirector>(root);
            CoachHighlighter highlighter = GetOrAddComponent<CoachHighlighter>(root);
            AddProviderForScene(scene.name, root);

            WarnOnDuplicatePanels();

            TutorialPanel panel = InstantiatePanel(root);
            if (panel == null)
            {
                Debug.LogError($"[CoachSceneSetup] {scene.name}: 패널을 만들지도 찾지도 못했다. 배선을 중단한다.");
                return;
            }

            Button closeButton = BuildCloseButton(panel.RootRectTransform);

            SerializedObject serialized = new SerializedObject(director);
            serialized.FindProperty("panel").objectReferenceValue = panel;
            serialized.FindProperty("highlighter").objectReferenceValue = highlighter;
            serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
            serialized.ApplyModifiedProperties();

            // MarkSceneDirty 는 더티 표시만 한다. 저장까지 하지 않으면 유저가 Ctrl+S 를
            // 누르지 않는 한 결과가 사라지고, 메뉴는 성공했다고 로그만 남긴다.
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log(
                $"[CoachSceneSetup] {scene.name} 배선 완료 후 저장했다. " +
                $"panel={Describe(panel)}, highlighter={Describe(highlighter)}, closeButton={Describe(closeButton)}");
        }

        /// <summary>
        /// 참조가 실제로 채워졌는지 로그에서 바로 보이게 한다. 메뉴가 조용히 절반만
        /// 하고 성공했다고 말하는 상황을 막으려는 것이다.
        /// </summary>
        private static string Describe(Object target)
        {
            return target != null ? target.name : "없음";
        }

        /// <summary>
        /// 예전 버전은 패널을 root 밑에서만 찾아서 재실행할 때마다 하나씩 더 만들었다.
        /// 그렇게 남은 중복은 사람이 지워야 하므로 조용히 넘어가지 않는다.
        /// </summary>
        private static void WarnOnDuplicatePanels()
        {
            int count = 0;
            foreach (TutorialPanel candidate in Object.FindObjectsOfType<TutorialPanel>(true))
            {
                if (candidate.gameObject.name == CoachPanelName)
                {
                    count++;
                }
            }

            if (count > 1)
            {
                Debug.LogWarning($"[CoachSceneSetup] {CoachPanelName} 이 {count} 개 있다. 하나만 남기고 지워라.");
            }
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

                Transform stale = FindDeepChild(contents.transform, ToggleName);
                if (stale != null)
                {
                    // 그냥 두고 나가면 옛 버전으로 만든 토글이 영영 그대로 남는다.
                    // 갈아끼워야 이 메뉴를 다시 돌리는 것으로 최신 모양이 된다.
                    Object.DestroyImmediate(stale.gameObject);
                    Debug.Log("[CoachSceneSetup] 기존 토글을 지우고 다시 만든다.");
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
        /// Unity 기본 토글은 흰 상자에 작은 체크 표시라 상태를 거의 말해 주지 않고,
        /// 이 프로젝트의 어떤 UI와도 어울리지 않는다. 대신 디자인 시스템으로 직접 만든다.
        /// 켜지면 행 전체가 primary 청록으로 차고, 옆의 글자가 상태를 말로 적어
        /// 색이 닿지 않는 유저에게도 알려 준다.
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
        /// 치울 수 없는 힌트는 없느니만 못하다. 패널 우측 상단에 닫기 버튼을 붙인다.
        /// 패널의 자식이라 패널과 함께 나타나고 사라진다.
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
                Debug.Log($"[CoachSceneSetup] {CloseButtonName} 이 이미 있어 그대로 쓴다.");
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
            // 패널은 root가 아니라 Canvas 아래에 붙는다. root 밑에서만 찾으면 재실행할 때마다
            // 패널이 하나씩 늘어나므로 씬 전체에서 이름으로 찾는다.
            foreach (TutorialPanel candidate in Object.FindObjectsOfType<TutorialPanel>(true))
            {
                if (candidate.gameObject.name == CoachPanelName)
                {
                    return candidate;
                }
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
            instance.name = CoachPanelName;
            Undo.RegisterCreatedObjectUndo(instance, "Create Coach Panel");

            // 훈수는 입력을 막지 않는다. 여기서 화면을 어둡게 하거나 잠그지 않는 이유다.
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
