namespace Script.Data.Localization
{
    public class LocaleSetButton : ButtonBase
    {
        public string localeCode;
        
        protected override void OnClickButton()
        {
            LocaleUtils.SetLanguageAsync(localeCode);
        }
    }
}