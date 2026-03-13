using Data.Adventures;
using Data.Adventures.Domain;
using Global.Button;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Adventures
{
    public class AdventureButton : ButtonBase
    {
        [SerializeField] private Image icon;
        
        private Adventure _adventure;
        
        protected override void OnClickButton()
        {
            CurrentAdventure.Instance.SetAdventure(_adventure);
            SceneManager.LoadScene("AdventureScene");
        }

        public void SetUp(bool isActive, Adventure adventure)
        {
            _adventure = adventure;
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