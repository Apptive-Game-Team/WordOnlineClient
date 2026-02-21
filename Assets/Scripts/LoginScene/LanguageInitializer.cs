using Scripts.Data.Localization;
using UnityEngine;

namespace Scripts.LoginScene
{
    public class LanguageInitializer : MonoBehaviour
    {
        private void Awake()
        {
            LocaleUtils.InitializeLanguageAsync();
        }
    }
}
