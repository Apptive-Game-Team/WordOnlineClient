using UnityEngine;
using UnityEngine.Localization;

namespace Data.Adventures.Local
{
    [CreateAssetMenu(menuName = "Game/Adventure/Stage")]
    public class AdventureStageScriptableObject : ScriptableObject
    {
        [SerializeField] private long stageId;
        [SerializeField] private Sprite backgroundImage;
        [SerializeField] private LocalizedString stageName;
    }
}