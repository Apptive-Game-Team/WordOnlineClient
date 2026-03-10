using Data.Adventures.Local;
using Global.Button;
using UnityEngine;
using UnityEngine.UI;

namespace Adventures
{
    public class AdventureButton : ButtonBase
    {
        [SerializeField] private Image icon;
        
        protected override void OnClickButton()
        {
            throw new System.NotImplementedException();
        }

        public void SetUp(bool isActive, AdventureScriptableObject adventure)
        {
            Debug.Log(adventure.IconImage);
            Debug.Log(icon);
            icon.sprite = adventure.IconImage;
            if (!isActive)
            {
                GetComponent<Button>().interactable = false;
                GetComponent<Image>().color = Color.gray;
                icon.color = Color.gray;
            }
        }
    }
}