using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Data.Adventures.Local
{
    [CreateAssetMenu(menuName = "Game/Adventure/New")]
    public class AdventureScriptableObject : ScriptableObject
    {
        [SerializeField] private long adventureId;
        [SerializeField] private Sprite iconImage;
        [SerializeField] private LocalizedString adventureName;
        
        [SerializeField] private List<AdventureStageScriptableObject> stages;
        
        public Sprite IconImage => iconImage;
        public long AdventureId => adventureId;
    }
}