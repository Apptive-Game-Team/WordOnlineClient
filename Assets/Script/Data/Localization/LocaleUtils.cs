using System;
using Script.Global;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace Script.Data.Localization
{
    public class LocaleUtils
    {
        public static async void SetLanguageAsync(string localeCode)
        {
            try
            {
                await LocalizationSettings.InitializationOperation.Task;
                Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
                if (locale != null)
                {
                    LocalizationSettings.SelectedLocale = locale;
                    PlayerPrefs.SetString("Language", localeCode);
                }
            }
            catch (Exception e)
            {
                WDebug.LogError(e);
            }
        }
        
        public static async void InitializeLanguageAsync()
        {
            try
            {
                await LocalizationSettings.InitializationOperation.Task;
                string savedLocaleCode = PlayerPrefs.GetString("Language", "en");
                Locale locale = LocalizationSettings.AvailableLocales.GetLocale(savedLocaleCode);
                if (locale != null)
                {
                    LocalizationSettings.SelectedLocale = locale;
                }
            }
            catch (Exception e)
            {
                WDebug.LogError(e);
            }
        }
    }
}