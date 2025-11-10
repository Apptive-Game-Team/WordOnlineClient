using Script.Data;
using UnityEngine;
using UnityEngine.EventSystems;

public class FieldSelector : MonoBehaviour
{
    CardInputSender cardInputSender;
    private GameObject currentAimObj;
    private GameObject currentRangeObj;
    private GameObject currentSkillIndicator;

    [SerializeField] private GameObject aimObject;
    [SerializeField] private GameObject rangeObject;
    [SerializeField] private GameObject lineSkillIndicator;
    [SerializeField] private GameObject circleSkillIndicator;

    void Start()
    {
        cardInputSender = FindObjectOfType<CardInputSender>();
        currentAimObj = Instantiate(aimObject);
        currentRangeObj = Instantiate(rangeObject);
        
        currentSkillIndicator = Instantiate(circleSkillIndicator);
        currentSkillIndicator.SetActive(false);
    }

    void Update()
    {
        if (!cardInputSender.IsFieldSelectMode())
        {
            if (currentAimObj.activeSelf)
            {
                currentAimObj.SetActive(false);
                currentRangeObj.SetActive(false);
                if (currentSkillIndicator != null) currentSkillIndicator.SetActive(false);
            }

            return;
        }
        
        if (!currentAimObj.activeSelf) currentAimObj.SetActive(true);
        currentRangeObj.SetActive(true);


        var md = LocalMagicData.GetMagicData(cardInputSender.GetMagicName());
        currentRangeObj.transform.localScale = Vector3.one * (2 * md.range);


        bool wantLine = md.name == "Shoot";
        GameObject wantedPrefab = wantLine ? lineSkillIndicator : circleSkillIndicator;


        if (currentSkillIndicator == null || !ReferenceEquals(currentSkillIndicatorPrefabRef, wantedPrefab))
        {
            if (currentSkillIndicator != null) Destroy(currentSkillIndicator);
            currentSkillIndicator = Instantiate(wantedPrefab);
            currentSkillIndicatorPrefabRef = wantedPrefab; 
        }


        if (!currentSkillIndicator.activeSelf) currentSkillIndicator.SetActive(true);

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentAimObj.transform.position = mouseWorldPos;

        currentRangeObj.transform.position = SceneContext.Me.Equals("RightPlayer")
            ? new Vector3(17, 5, 0)
            : new Vector3(1, 5, 0);

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonUp(0))
        {
            cardInputSender.SendInput(mouseWorldPos);
            PlayerFeedbackController.Instance.UseMagicFeedback();
            currentAimObj.SetActive(false);
            currentRangeObj.SetActive(false);
            currentSkillIndicator.SetActive(false);
            cardInputSender.SetExpectedMagicUI();
        }
    }
    
    private GameObject currentSkillIndicatorPrefabRef;
}