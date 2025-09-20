using Script.Data;
using UnityEngine;

namespace Script.MagicBookScene
{
    public class MagicInfoFactory : MonoBehaviour
    {
        [SerializeField] private Transform magicInfoParent;
        [SerializeField] private GameObject magicInfoPrefab;
        
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
        
        private void CreateMagicInfo(CombinedMagicData data)
        {
            var magicInfoObj = Instantiate(magicInfoPrefab, magicInfoParent);
            var magicInfo = magicInfoObj.GetComponent<MagicInfo>();
            magicInfo.Init(data);
        }
    }
}