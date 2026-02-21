namespace Scripts.Global.Button
{
    public abstract class AsyncButtonBase : DisableableButtonBase
    {
        public override void ButtonEvent()
        {
            if (!isActive)
            {
                return;
            }
            base.ButtonEvent();
            SetButton();
        }
    }
}