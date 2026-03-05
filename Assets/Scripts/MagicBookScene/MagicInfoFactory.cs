using System.Collections.Generic;
using Data.Magic;
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
            userMagicApiClient.GetUserMagic((response) =>
            {
                CreateAllMagicInfo(response.magicIds);
            });
        }
        
        private void CreateAllMagicInfo(List<long> userMagicIds = null)
        {
            foreach (var data in LocalCombinedMagicData.dataList)
            {
                bool active = userMagicIds.Contains(data.id);
                CreateMagicInfo(data, active);
            }
        }
        
        private void OnClickMagicButton(CombinedMagicData data)
        {
            magicInfo.Init(data);
        }
        
        private void CreateMagicInfo(CombinedMagicData data, bool active = true)
        {
            var magicInfoObj = Instantiate(magicInfoPrefab, magicInfoParent);
            
            var magicInfo = magicInfoObj.GetComponent<MagicButton>();
            magicInfo.Init(data);
            magicInfo.SetActive(active);
            
            if (active)
            {
                magicInfo.OnClick += OnClickMagicButton;
            }
        }
    }
}