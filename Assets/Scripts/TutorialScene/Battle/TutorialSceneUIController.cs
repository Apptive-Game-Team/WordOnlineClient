using System.Collections.Generic;
using Data;
using Data.Magic;
using GameScene;
using GameScene.Card;
using GameScene.Player;
using GameScene.PopupBook;
using GameScene.ServedObjectComponent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TutorialScene
{
    public class TutorialSceneUIController : MonoBehaviour
    {
        public static TutorialSceneUIController Instance;
        
        [SerializeField] private TextMeshProUGUI leftUserIDText;
        [SerializeField] private TextMeshProUGUI rightUserIDText;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private Slider manaSlider;
    
        [SerializeField] private Slider leftUserHpSlider;
        [SerializeField] private Slider rightUserHpSlider;
    
        private int rightUserHp = 100;
    
        [SerializeField] private TutorialCardUI cardUIPrefab;
        [SerializeField] private GameObject lowerBar;

        [SerializeField] private CardImageMapper cardImageMapper;

        [SerializeField] private ExpectedMagicUI expectedMagicUI;
    
        [SerializeField] private MagicFailEffecter leftUserMagicFailEffecter;
        [SerializeField] private MagicFailEffecter rightUserMagicFailEffecter;
    
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            AttachPopupBookPresenter(GameObject.Find("LeftPlayer"));
            AttachPopupBookPresenter(GameObject.Find("RightPlayer"));

            if (leftUserIDText == null || rightUserIDText == null) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        if (Global.SceneContext.MatchInfo != null)
        {
            leftUserIDText.text = Global.SceneContext.MatchInfo.leftUser.name;
            rightUserIDText.text = Global.SceneContext.MatchInfo.rightUser.name;
        }
#endif
        }

        private static void AttachPopupBookPresenter(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            ServedObject servedObject = target.GetComponent<ServedObject>();
            if (servedObject == null)
            {
                servedObject = target.AddComponent<ServedObject>();
            }

            PopupBookVisualPresenter.Attach(servedObject);
            servedObject.BindListeners();
        }

        public void UpdateUserHps(int leftUserHp, int rightUserHp)
        {
            if (leftUserHpSlider.value > leftUserHp)
            {
                Transform leftPlayerTr = GameObject.Find("LeftPlayer").transform;
                DOTweenAction.BounceMob(leftPlayerTr.GetChild(0));
                DamagedObjectEffect.SetSelfDestroyEffect("HitEffect",leftPlayerTr);
            }
            else if (rightUserHpSlider.value > rightUserHp)
            {
                Transform rightPlayerTr = GameObject.Find("RightPlayer").transform;
                DOTweenAction.BounceMob(rightPlayerTr.GetChild(0));
                DamagedObjectEffect.SetSelfDestroyEffect("HitEffect", rightPlayerTr);
            }
        
            leftUserHpSlider.value = leftUserHp;
            rightUserHpSlider.value = rightUserHp;
        
            leftUserIDText.text = $"ME\n HP: {leftUserHp}";
            rightUserIDText.text = $"ENEMY\n HP: {rightUserHp}";
        }

        public void UpdateMana(int mana)
        {
            if (manaText == null || manaSlider == null) return;
            manaText.text = mana.ToString();
            manaSlider.value = mana;
        }

        public void AddCard(string cardname)
        {
            if (lowerBar == null || cardUIPrefab == null || cardImageMapper == null) return;
            TutorialCardUI cardUI = Instantiate(cardUIPrefab, lowerBar.transform);
            cardUI.transform.GetChild(2).GetComponent<Image>().sprite = cardImageMapper.GetCardImage(cardname);
            cardUI.Init(cardname);
        }

        public void TrySetExpectedMagicUI(IList<CardType> recipe)
        {
            if (expectedMagicUI == null) return;
            LocalCombinedMagicData.TryGetByRecipe(recipe, out CombinedMagicData data);
            if (data != null)
            {
                expectedMagicUI.SetImage(data.GetSprite());
                return;
            }
            expectedMagicUI.SetImage(null);
        }
    
        public void PlayMagicFailEffect()
        {
            if (leftUserMagicFailEffecter != null)
            {
                leftUserMagicFailEffecter.Trigger();
            }
            else if (rightUserMagicFailEffecter != null)
            {
                rightUserMagicFailEffecter.Trigger();
            }
        }

        public void ClearAllCards()
        {
            for (int i = lowerBar.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(lowerBar.transform.GetChild(i).gameObject);
            }
        }

        public void HPMocking(int hpReduce)
        {
            rightUserHp-= hpReduce;
            UpdateUserHps(100, rightUserHp);
            if(rightUserHp <= 0) BattleTutorialManager.Instance.NotifyEnemyDead();
        }
    }
}
