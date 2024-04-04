using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdmobManager : Singleton<AdmobManager>
{
    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    const string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
const string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
const string adUnitId = "unused";
#endif

    private RewardedAd _rewardedAd;

    public void Init()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => {
            LoadRewardedAd();
        });
    }
    private void LoadRewardedAd()
    {
        // Load a rewarded ad
        RewardedAd.Load(adUnitId, new AdRequest(),
            (RewardedAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    Debug.Log("Rewarded ad failed to load with error: " +
                               loadError.GetMessage());
                    return;
                }
                else if (ad == null)
                {
                    Debug.Log("Rewarded ad failed to load.");
                    return;
                }

                Debug.Log("Rewarded ad loaded.");
                _rewardedAd = ad;
            });
    }
    public void ShowRewardedAd()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("Rewarded ad granted a reward: " +
                        reward.Amount);
            });
        }
        else
        {
            Debug.Log("Rewarded ad cannot be shown.");
        }
    }

}
