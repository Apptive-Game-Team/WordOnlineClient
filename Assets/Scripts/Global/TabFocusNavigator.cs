using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Global
{
    /// <summary>
    /// Tab / Shift+Tab 으로 UI 포커스를 이동시킨다.
    /// StandaloneInputModule 은 Horizontal/Vertical 축만 네비게이션에 사용하므로 Tab 은 기본적으로 아무 동작도 하지 않고,
    /// 포커스를 가진 InputField 가 키 이벤트를 소비해 네비게이션 이벤트 자체가 발생하지 않는다.
    /// 그래서 EventSystem 을 거치지 않고 Input 을 직접 폴링한 뒤 Selectable 의 자동 네비게이션을 호출한다.
    /// </summary>
    public class TabFocusNavigator : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            GameObject host = new GameObject(nameof(TabFocusNavigator));
            host.AddComponent<TabFocusNavigator>();
            DontDestroyOnLoad(host);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab)) return;
            if (EventSystem.current == null) return;

            bool backward = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            Selectable next = FindNext(backward);
            if (next == null) return;

            Focus(next);
        }

        private static Selectable FindNext(bool backward)
        {
            Selectable current = CurrentSelectable();
            if (current == null) return Edge(backward);

            // Navigation.mode 가 Automatic 인 Selectable 은 기하학적 위치로 다음 대상을 찾는다.
            // 방향키 네비게이션과 같은 규칙이라 이동 결과가 예측 가능하다.
            Selectable next = backward ? current.FindSelectableOnUp() : current.FindSelectableOnDown();
            return next != null ? next : Edge(backward);
        }

        private static Selectable CurrentSelectable()
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected == null) return null;

            Selectable selectable = selected.GetComponent<Selectable>();
            return IsFocusable(selectable) ? selectable : null;
        }

        /// <summary>순환용 끝단. 정방향이면 화면 최상단, 역방향이면 최하단 요소를 반환한다.</summary>
        private static Selectable Edge(bool backward)
        {
            Selectable edge = null;
            float edgeY = 0f;

            Selectable[] all = Selectable.allSelectablesArray;
            foreach (Selectable candidate in all)
            {
                if (!IsFocusable(candidate)) continue;

                float y = candidate.transform.position.y;
                if (edge != null && (backward ? y >= edgeY : y <= edgeY)) continue;

                edge = candidate;
                edgeY = y;
            }

            return edge;
        }

        private static bool IsFocusable(Selectable selectable)
        {
            if (selectable == null) return false;
            if (!selectable.IsActive() || !selectable.IsInteractable()) return false;

            return selectable.navigation.mode != Navigation.Mode.None;
        }

        private static void Focus(Selectable target)
        {
            target.Select();

            // Select() 만으로는 캐럿이 생기지 않는 경우가 있어 입력 필드는 명시적으로 활성화한다.
            switch (target)
            {
                case InputField inputField:
                    inputField.ActivateInputField();
                    break;
                case TMP_InputField tmpInputField:
                    tmpInputField.ActivateInputField();
                    break;
            }
        }
    }
}
