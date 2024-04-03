using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmobTest : MonoBehaviour
{
    public void OnClickBtnShowAD()
    {
        GoogleAdMob.Instance.ShowRewardedAd();
    }
}
