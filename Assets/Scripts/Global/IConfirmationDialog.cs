using System;
using UnityEngine.Localization;

namespace Global
{
    public interface IConfirmationDialog
    {
        void Show(LocalizedString message, Action onConfirm, Action onCancel = null);
    }
}
