using Data.Adventures;
using TMPro;
using UnityEngine;

namespace Adventures
{
    public class CurrentAdventureNameSetter : MonoBehaviour
    {
        [SerializeField] private TMP_Text name;
    
        private void Start()
        {
            var adventure = CurrentAdventure.Instance.Adventure;
            if (adventure != null)
            {
                name.text = adventure.Name;
            }
        }
    }
}