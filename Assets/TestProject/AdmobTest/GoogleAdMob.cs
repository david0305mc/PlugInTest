using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class GoogleAdMob : SingletonMonoBehaviour<GoogleAdMob>
{
    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    const string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
const string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
const string adUnitId = "unused";
#endif

    // Start is called before the first frame update
    void Start()
    {
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // This callback is called once the MobileAds SDK is initialized.
            LoadRewardedAd();
        });
    }


    private RewardedAd _rewardedAd;

    /// <summary>
    /// Loads the rewarded ad.
    /// </summary>
    public void LoadRewardedAd()
    {    // Initialize an InterstitialAd.
        _rewardedAd = new RewardedAd(adUnitId);
        // Called when an ad request has successfully loaded.
        _rewardedAd.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request has failed to load.
        _rewardedAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        _rewardedAd.LoadAd(request);
    }
    private void HandleOnAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("Rewarded ad loaded.");
    }

    private void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        if (args != null)
        {
            Debug.Log("Rewarded ad failed to load with error: " +
                       args.LoadAdError.GetMessage());
        }
    }

    public void ShowRewardedAd()
    {
        if (_rewardedAd != null )
        {
            _rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
            _rewardedAd.Show();

        }
        else
        {
            Debug.Log("Rewarded ad is not ready yet.");
        }
    }
    public void HandleUserEarnedReward(object sender, Reward reward)
    {
        Debug.Log("Rewarded ad granted a reward: " +
                   reward.Amount);
    }
}