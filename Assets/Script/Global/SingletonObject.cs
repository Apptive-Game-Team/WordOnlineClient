namespace Script.Global
{
    public class SingletonObject<T> : LocalSingletonObject<T> where T : LocalSingletonObject<T>
    {
        protected override void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}