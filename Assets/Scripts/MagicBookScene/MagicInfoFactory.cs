using System.Collections.Generic;
using Data.Magic;
using Global;
using TutorialScene;
using UnityEngine;

namespace MagicBookScene
{
    public class MagicInfoFactory : MonoBehaviour
    {
        [SerializeField] private Transform magicInfoParent;
        [SerializeField] private GameObject magicInfoPrefab;
        [SerializeField] private MagicInfo magicInfo;
        
        [SerializeField] private UserMagicApiClient userMagicApiClient;
        
        private void Awake()
        {
            var savedMagicJson = PlayerPrefs.GetString(MagicInfoDataSource.PlayerPrefsKeyName, string.Empty);
            WDebug.Log($"[MagicInfoFactory] Saved magic json: {savedMagicJson}");

            MagicInfoDataSource.Instance.GetMagics(_ =>
            {
                userMagicApiClient.GetUserMagic(response =>
                {
                    CreateAllMagicInfo(response?.magicIds);
                });
            });
        }
        
        private void CreateAllMagicInfo(List<long> userMagicIds = null)
        {
            userMagicIds ??= new List<long>();
            foreach (var data in LocalCombinedMagicData.GetEffectiveDataList())
            {
                bool active = userMagicIds.Contains(data.id);
                CreateMagicInfo(data, active);
            }
        }
        
        private void OnClickMagicButton(CombinedMagicData data)
        {
            magicInfo.Init(data);
            FindObjectOfType<MagicBookTutorialController>()?.NotifyMagicSelected();
        }
        
        private void CreateMagicInfo(CombinedMagicData data, bool active = true)
        {
            var magicInfoObj = Instantiate(magicInfoPrefab, magicInfoParent);
            
            var magicButton = magicInfoObj.GetComponent<MagicButton>();
            magicButton.Init(data);
            magicButton.SetActive(active);
            
            if (active)
            {
                magicButton.OnClick += OnClickMagicButton;
            }
        }
    }
}
