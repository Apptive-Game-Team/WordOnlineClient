using System.Collections.Generic;
using Data.Adventures.Domain;
using Data.Adventures.Local;
using Unity.VisualScripting;
using UnityEngine;
using State = Data.Adventures.State;

namespace Adventures
{
    public class AdventureIconFactory : MonoBehaviour
    {
        [SerializeField] private AdventureDataSource adventureDateSource;
        [SerializeField] private GameObject adventureIconPrefab;
        [SerializeField] private GameObject iconParent;
        
        private void Start()
        {
            adventureDateSource.GetAdventures(CreateAdventureIcons);
        }
        
        private void CreateAdventureIcons(List<Adventure> adventureData)
        {
            foreach (var adventure in adventureData)
            {
                var icon = Instantiate(adventureIconPrefab, iconParent.transform);
                var button = icon.GetComponent<AdventureButton>();
                button.SetUp(adventure.State != State.INACTIVE, adventure);
            }
        }
    }
}