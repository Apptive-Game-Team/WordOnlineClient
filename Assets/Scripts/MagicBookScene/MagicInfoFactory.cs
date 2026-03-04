using Data;
using Data.Magic;
using UnityEngine;

namespace MagicBookScene
{
    public class MagicInfoFactory : MonoBehaviour
    {
        [SerializeField] private Transform magicInfoParent;
        [SerializeField] private GameObject magicInfoPrefab;
        [SerializeField] private MagicInfo magicInfo;
        
        private void Start()
        {
            CreateAllMagicInfo();
        }
        
        private void CreateAllMagicInfo()
        {
            foreach (var data in LocalCombinedMagicData.dataList)
            {
                CreateMagicInfo(data);
            }
        }
        
        private void OnClickMagicButton(CombinedMagicData data)
        {
            magicInfo.Init(data);
        }
        
        private void CreateMagicInfo(CombinedMagicData data)
        {
            var magicInfoObj = Instantiate(magicInfoPrefab, magicInfoParent);
            var magicInfo = magicInfoObj.GetComponent<MagicButton>();
            magicInfo.Init(data);
            magicInfo.OnClick += OnClickMagicButton;
        }
    }
}