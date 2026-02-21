using Global.Button;

namespace Data.Localization
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