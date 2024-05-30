using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmobTest : MonoBehaviour
{
    [SerializeField] private GameObject uiRoot;
    private void Awake()
    {
        uiRoot.SetActive(false);
        //AdmobManager.Instance.Initialize();
        AdmobManager.Instance.Initialize(()=> {
            uiRoot.SetActive(true);
        });
    }
    public void OnClickBtnShowAD()
    {
        AdmobManager.Instance.Show(ADType.Gacha);
    }

    public void OnClickBtnInspector()
    {
        MobileAds.OpenAdInspector(error => {
            // Error will be set if there was an issue and the inspector was not displayed.
        });
    }
}
