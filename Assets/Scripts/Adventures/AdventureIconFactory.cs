using System.Collections.Generic;
using Data.Adventures.Local;
using Unity.VisualScripting;
using UnityEngine;

namespace Adventures
{
    public class AdventureIconFactory : MonoBehaviour
    {
        [SerializeField] private AdventureDateSource adventureDateSource;
        [SerializeField] private GameObject adventureIconPrefab;
        [SerializeField] private GameObject iconParent;
        
        private void Start()
        {
            adventureDateSource.GetAdventures(CreateAdventureIcons);
        }
        
        private void CreateAdventureIcons(List<(bool isActive, AdventureScriptableObject adventure)> adventureData)
        {
            foreach (var (isActive, adventure) in adventureData)
            {
                var icon = Instantiate(adventureIconPrefab, iconParent.transform);
                var button = icon.GetComponent<AdventureButton>();
                button.SetUp(isActive, adventure);
            }
        }
    }
}