using System.Collections;
using UnityEngine;

public class BarController : MonoBehaviour
{
    
    private RectTransform _rectTransform;
    [SerializeField] private bool isActive = false;
    private bool lastActive = false;

    [SerializeField] private CardInputSender cardInputSender;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (CheckMouseOverBar() && !cardInputSender.IsFieldSelectMode)
        {
            isActive = true;
        }
        else
        {
            isActive = false;
        }
        
        if (lastActive != isActive)
        {
            lastActive = isActive;
            SetBarActive(isActive);
        }
    }

    private void SetBarActive(bool active)
    {
        if (active)
        {
            StartCoroutine(MoveBar(true));
        }
        else
        {
            StartCoroutine(MoveBar(false));
        }
    }

    private bool CheckMouseOverBar()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.y < 300f;
    }
    
    private IEnumerator MoveBar(bool up, float duration = 0.5f)
    {
        
        Vector2 startPosition = _rectTransform.anchoredPosition;
        Vector2 endPosition = up ? 
                new Vector2(0, 0) : 
                new Vector2(0, -300f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            _rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _rectTransform.anchoredPosition = endPosition; // Ensure final position is set
    }
}
