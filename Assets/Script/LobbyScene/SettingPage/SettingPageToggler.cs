using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPage : MonoBehaviour
{
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject settingPage;
    
    public void SetSettingPageActive(bool open)
    {
        settingPage.SetActive(open);
        closeButton.SetActive(open);
    }
}
