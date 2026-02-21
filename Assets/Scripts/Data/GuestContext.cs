namespace Scripts.Data
{
    public static class GuestContext
    {
        public static string GuestPassword
        {
            get; set;
        }
        
        public static bool IsGuest => !string.IsNullOrEmpty(GuestPassword);
        
        public static void ClearGuestInfo()
        {
            GuestPassword = null;
        }
    }
}