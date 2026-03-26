using System;
using Global.Button;
using TMPro;
using UnityEngine;

namespace Admin
{
    public class DebugItemUI : ButtonBase
    {
        [SerializeField] private TMP_Text nameText;

        private Action onClickAction;

        public void SetItem(string name, Action onClick)
        {
            nameText.text = name;
            onClickAction = onClick;
        }

        protected override void OnClickButton()
        {
            onClickAction?.Invoke();
        }
    }
}
