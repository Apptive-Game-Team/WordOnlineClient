using System;
using System.Collections.Generic;
using System.Linq;
using Data.Adventures;
using Data.Adventures.Local;
using UnityEngine;

namespace Adventures
{
    public class AdventureDateSource : MonoBehaviour
    {
        [SerializeField] private AdventureClient adventureApiClient;
        [SerializeField] private List<AdventureScriptableObject> localAdventureData;
        
        public List<AdventureScriptableObject> LocalAdventureData => localAdventureData;
        
        
        public void GetAdventures(Action<List<(bool isActive, AdventureScriptableObject adventure)>> onSuccess)
        {
            StartCoroutine(
                    adventureApiClient.GetAdventure(response =>
                    {
                        
                        var list = localAdventureData
                            .GroupJoin(
                                response.adventures,
                                local => local.AdventureId,
                                api => api.id,
                                (local, apiGroup) => new { local, apiGroup }
                            )
                            .Select((group, i) =>
                            {
                                var apiData = group.apiGroup.FirstOrDefault();
                                
                                bool isActive = apiData != null && apiData.state != "INACTIVE";
                                
                                return (isActive, group.local);
                            })
                            .ToList();

                        onSuccess.Invoke(list);
                    }));
        }
    }
}