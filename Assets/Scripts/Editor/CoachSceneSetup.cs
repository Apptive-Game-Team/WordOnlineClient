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
        private const string PanelPrefabPath = "Assets/Prefabs/UI/Tutorial/TutorialMessagePanel.prefab";
        private const string SettingsPrefabPath = "Assets/Prefabs/UI/Lobby/Panal.prefab";
        private const string SoundSlidersName = "SoundSliders";
        private const string ToggleName = "CoachHintToggle";

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

            SerializedObject serialized = new SerializedObject(director);
            serialized.FindProperty("panel").objectReferenceValue = panel;
            serialized.FindProperty("highlighter").objectReferenceValue = highlighter;
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

                Transform existing = FindDeepChild(contents.transform, ToggleName);
                if (existing != null)
                {
                    Debug.Log("[CoachSceneSetup] The settings panel already carries the hint toggle.");
                    return;
                }

                GameObject toggleObject = DefaultControls.CreateToggle(new DefaultControls.Resources());
                toggleObject.name = ToggleName;
                toggleObject.transform.SetParent(parent, false);

                ReplaceLabelWithTmp(toggleObject);

                CoachDataSetter setter = contents.GetComponent<CoachDataSetter>();
                if (setter == null)
                {
                    setter = contents.AddComponent<CoachDataSetter>();
                }

                SerializedObject serialized = new SerializedObject(setter);
                serialized.FindProperty("coachToggle").objectReferenceValue = toggleObject.GetComponent<Toggle>();
                serialized.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(contents, SettingsPrefabPath);
                Debug.Log("[CoachSceneSetup] Added the hint toggle to the settings panel. Position and label it to taste.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
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

        /// <summary>
        /// DefaultControls builds a legacy Text label, but every other label in
        /// this project is TextMeshPro and the localization tables target it.
        /// </summary>
        private static void ReplaceLabelWithTmp(GameObject toggleObject)
        {
            Text label = toggleObject.GetComponentInChildren<Text>(true);
            if (label == null)
            {
                return;
            }

            GameObject labelObject = label.gameObject;
            Object.DestroyImmediate(label);

            TextMeshProUGUI tmp = labelObject.AddComponent<TextMeshProUGUI>();
            tmp.text = "Hints";
            tmp.fontSize = 24f;
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
