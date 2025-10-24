using Script.Data.Localization;
using UnityEngine;

public class LanguageInitializer : MonoBehaviour
{
    private void Awake()
    {
        LocaleUtils.InitializeLanguageAsync();
    }
}
