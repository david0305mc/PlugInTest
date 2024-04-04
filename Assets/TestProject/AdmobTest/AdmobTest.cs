using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmobTest : MonoBehaviour
{
    private void Awake()
    {
        //AdmobManager.Instance.Initialize();
        AdmobManager.Instance.Initialize();
    }
    public void OnClickBtnShowAD()
    {
        AdmobManager.Instance.Show(AdType.Gacha);
    }
}
