using UnityEngine;

namespace TutorialScene
{
    public class ManaMocker : MonoBehaviour
    {
        private int curMana = 0;

        private void Start()
        {
            InvokeRepeating("ChargeMana",0,0.1f); 
        }

        void ChargeMana()
        {
            if (curMana < 120) curMana++;
            TutorialSceneUIController.Instance.UpdateMana(curMana);
        }

        public void UseMana(int amount)
        {
            curMana -= amount;
            TutorialSceneUIController.Instance.UpdateMana(curMana);
        }
    }
}
