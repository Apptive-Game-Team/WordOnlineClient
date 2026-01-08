using System.Collections;
using System.Collections.Generic;
using Script.Data;
using Script.GameScene;
using Script.GameScene.ServedObjectComponent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUIController : MonoBehaviour
{
    public static GameSceneUIController Instance;
    
    [SerializeField] private TextMeshProUGUI leftUserIDText;
    [SerializeField] private TextMeshProUGUI rightUserIDText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private Slider manaSlider;
    
    [SerializeField] private Slider leftUserHpSlider;
    [SerializeField] private Slider rightUserHpSlider;
    
    [SerializeField] private CardUI cardUIPrefab;
    [SerializeField] private GameObject lowerBar;
    
    [SerializeField] private TextMeshProUGUI textSystemMsg;

    [SerializeField] private CardImageMapper cardImageMapper;

    [SerializeField] private ExpectedMagicUI expectedMagicUI;
    
    [SerializeField] private MagicHelperUI magicHelperUI;
    
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
#if UNITY_WEBGL && !UNITY_EDITOR
        leftUserIDText.text = SceneContext.MatchInfo.leftUser.name;
        rightUserIDText.text = SceneContext.MatchInfo.rightUser.name;
#endif
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
        
        leftUserIDText.text = $"{SceneContext.MatchInfo.leftUser.name}\n HP: {leftUserHp}";
        rightUserIDText.text = $"{SceneContext.MatchInfo.rightUser.name}\n HP: {rightUserHp}";
    }

    public void UpdateMana(int mana)
    {
        if (manaText == null || manaSlider == null) return;
        manaText.text = mana.ToString();
        manaSlider.value = mana;
    }

    public void AddCard(string cardname)
    {
        if (lowerBar == null || cardUIPrefab == null || magicHelperUI == null || cardImageMapper == null) return;
        CardUI cardUI = Instantiate(cardUIPrefab, lowerBar.transform);
        cardUI.transform.GetChild(2).GetComponent<Image>().sprite = cardImageMapper.GetCardImage(cardname);
        cardUI.Init(cardname);
        magicHelperUI.RefreshSuggestions();
    }

    public List<string> GetAllCards()
    {
        if (lowerBar == null) return new List<string>();
        List<string> cardNames = new List<string>();
        foreach (Transform child in lowerBar.transform)
        {
            cardNames.Add(child.GetComponent<CardUI>().CardName);
        }
        return cardNames;
    } 

    public void TrySetExpectedMagicUI(IList<CardType> recipe)
    {
        if (expectedMagicUI == null) return;
        CombinedMagicResolver.TryResolve(recipe, out CombinedMagicData data);
        if (data != null)
        {
            expectedMagicUI.SetImage(data.GetSprite());
            return;
        }
        expectedMagicUI.SetImage(null);
    }
    
    public void PlayMagicFailEffect()
    {
        if (SceneContext.Me == "LeftPlayer")
        {
            leftUserMagicFailEffecter.Trigger();
        }
        else if (SceneContext.Me == "RightPlayer")
        {
            rightUserMagicFailEffecter.Trigger();
        }
    }
}
