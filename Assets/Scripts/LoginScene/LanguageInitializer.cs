using Data.Localization;
using UnityEngine;

namespace LoginScene
{
    public class LanguageInitializer : MonoBehaviour
    {
        private void Awake()
        {
            LocaleUtils.InitializeLanguageAsync();
        }
    }
}
