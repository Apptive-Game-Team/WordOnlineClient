using UnityEngine;
using UnityEngine.UI;

public class DecorationIconView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject equippedMarker; // 체크 표시 같은 거
    [SerializeField] private Button button;

    private DecorationData meta;
    private System.Action<DecorationData> onClicked;

    public void Init(DecorationData meta, bool isEquipped, System.Action<DecorationData> onClicked)
    {
        this.meta = meta;
        this.onClicked = onClicked;

        iconImage.sprite = meta.iconSprite;
        SetEquipped(isEquipped);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => this.onClicked?.Invoke(this.meta));
    }

    public void SetEquipped(bool equipped)
    {
        if (equippedMarker != null)
            equippedMarker.SetActive(equipped);
    }

    public DecorationData Meta => meta;
}