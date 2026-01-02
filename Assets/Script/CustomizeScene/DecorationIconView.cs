using Script.CustomizeScene.dto;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationIconView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject equippedMarker; // 체크 표시 같은 거
    [SerializeField] private Button button;
    
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private TMP_Text requireText;
    [SerializeField] private TMP_Text progressText; 
    
    private DecorationData meta;
    private System.Action<DecorationData> onClicked;
    
    private bool isLocked = false;

    public void Init(DecorationData meta, bool isEquipped, System.Action<DecorationData> onClicked, QuestState questState = null)
    {
        this.meta = meta;
        this.onClicked = onClicked;

        if (lockIcon != null && questState == null)
            lockIcon.SetActive(false);
        else if (questState != null)
            SetLock(questState);
        
        iconImage.sprite = meta.iconSprite;
        SetEquipped(isEquipped);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClicked?.Invoke(this.meta));
    }

    private void SetLock(QuestState questState)
    {
        isLocked = true;
        
        if (lockIcon != null)
            lockIcon.SetActive(true);
        
        if (questState.state == "PENDING")
        {
            return;
        }
        
        if (requireText != null)
            requireText.text = $"{questState.requireValue}승";
        
        if (progressText != null)
            progressText.text = $"{questState.progress}/{questState.requireValue}";
        
    }

    public void SetEquipped(bool equipped)
    {
        if (isLocked) return;
        if (equippedMarker != null)
            equippedMarker.SetActive(equipped);
    }

    public DecorationData Meta => meta;
}