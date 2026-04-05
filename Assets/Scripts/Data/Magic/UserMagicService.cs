using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data.Magic
{
    public class UserMagicService : MonoBehaviour
    {
        [SerializeField] private UserMagicApiClient apiClient;

        public void GetCombinedMagicData(Action<List<CombinedMagicData>> callback)
        {
            apiClient.GetUserMagic(userMagicResponse =>
            {
                if (userMagicResponse == null)
                {
                    callback.Invoke(null);
                    return;
                }

                List<CombinedMagicData> list = LocalCombinedMagicData.GetEffectiveDataList()
                    .Where((data) => userMagicResponse.magicIds.Contains(data.id))
                    .ToList();
                callback.Invoke(list);
            });
        }
    }
}