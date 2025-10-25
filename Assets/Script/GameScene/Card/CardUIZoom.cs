using Script.Data;
using Script.GameScene;
using UnityEngine;

public class CardUIZoom : MonoBehaviour
{
    public static CardUIZoom Instance { get; private set; }
    
    [SerializeField] private ZoomedCardUI zoomedCardUI;
    
    private void Awake()
    {
        Instance = this;
        zoomedCardUI.Hide();
    }
    
    public void Show(CardUI cardUI)
    {
        Debug.Log($"Showing zoomed card for {cardUI.CardName} at position {cardUI.transform.position}, {Screen.width / 2}");
        zoomedCardUI.Show(cardUI);
    }
    
    public void Hide()
    {
        zoomedCardUI.Hide();
    }
}
