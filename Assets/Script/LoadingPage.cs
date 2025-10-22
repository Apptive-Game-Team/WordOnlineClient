using Script.Data;
using Script.Global;
using UnityEngine;

public class LoadingPage : MonoBehaviour
{
    
    [SerializeField] private GameObject loadingIndicator;
    
    public static LoadingPage Instance { 
        get; 
        private set; 
    }
    
    private bool isLoading = false;
    
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            isLoading = value;
            loadingIndicator.SetActive(isLoading);
            WDebug.Log("Loading state changed: " + isLoading);
        }
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }
}