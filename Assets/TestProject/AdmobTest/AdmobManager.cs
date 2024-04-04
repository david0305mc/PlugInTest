using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum AdType
{ 
    ReceiveGoods,
    Gacha,
    RandomReward,
    ReduceTime,
}

public enum EResult : int
{
    None,
    Success,
    Error,
}


public class AdmobManager : Singleton<AdmobManager>
{
    readonly Dictionary<AdType, string> AdUnitIDDic = new Dictionary<AdType, string>() 
    {
        {
            AdType.ReceiveGoods,
#if UNITY_ANDROID
            "ca-app-pub-3940256099942544/5224354917"
#elif UNITY_IPHONE
            "ca-app-pub-3940256099942544/1712485313"
#else
            "ca-app-pub-3940256099942544/1712485313"
#endif
        },
        {
            AdType.Gacha,
#if UNITY_ANDROID
            "ca-app-pub-3940256099942544/5224354917"
#elif UNITY_IPHONE
            "ca-app-pub-3940256099942544/1712485313"
#else
            "ca-app-pub-3940256099942544/1712485313"
#endif
        },
        {

            AdType.ReduceTime,
#if UNITY_ANDROID
            "ca-app-pub-3940256099942544/5224354917"
#elif UNITY_IPHONE
            "ca-app-pub-3940256099942544/1712485313"
#else
            "ca-app-pub-3940256099942544/1712485313"
#endif
        },
        {

            AdType.RandomReward,
#if UNITY_ANDROID
            "ca-app-pub-3940256099942544/5224354917"
            
#elif UNITY_IPHONE
            "ca-app-pub-3940256099942544/1712485313"
#else
            "ca-app-pub-3940256099942544/1712485313"
#endif
        }
    };

    private Dictionary<AdType, AdmobUnit> dicAdmobUnit = new Dictionary<AdType, AdmobUnit>();
    private bool _initialized = false;
    public void Initialize()
    {
#if UNITY_EDITOR
        _initialized = true;
#endif
        if (_initialized)
            return;

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => {
            Load(); 
        });
    }
    private void Load()
    {
        foreach (var item in AdUnitIDDic)
        {
            AdmobUnit admobUnit = new AdmobUnit(item.Value);
            dicAdmobUnit.Add(item.Key, admobUnit);
        }
    }
    public UniTask<EResult> Show(AdType _type)
    {
        return dicAdmobUnit[_type].Show();
    }
}

public class AdmobUnit
{
    private string adUnitId;
    private List<AdmobAd> _adCache;
    private int cacheSize = 3;
    public AdmobUnit(string _adUnitID)
    {
        adUnitId = _adUnitID;
        _adCache = new List<AdmobAd>();
        Load();
    }

    private void Load()
    {
        while (_adCache.Count < cacheSize)
        {
            Create();
        }
    }

    private void Create()
    {
        _adCache.Add(new AdmobAd(adUnitId));
    }

    private void Remove(AdmobAd ad)
    {
        ad.Release();
        _adCache.Remove(ad);
    }
    public async UniTask<EResult> Show()
    {
        try
        {
            // Touch Block
            AdmobAd ad = await Get();
            if (ad == null)
                return EResult.Error;

            UniTaskCompletionSource<EResult> completeionSource = new UniTaskCompletionSource<EResult>();
            ad.Show(result=> {
                Remove(ad);
                completeionSource.TrySetResult(result);
            });
            return await completeionSource.Task;
        }
        finally
        {
            // touch Unblock
            UpdateCache();
        }
    }
    async UniTask<AdmobAd> Get()
    {
        AdmobAd ad = null;
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfterSlim(TimeSpan.FromSeconds(10));
        try
        {
            while (true)
            {
                ad = _adCache.Find(x => x.IsLoaded);
                if (ad != null)
                    break;

                UpdateCache();
                await UniTask.Yield(cts.Token);
            }
        }
        finally
        {
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        }

        return ad;
    }

    private void UpdateCache()
    {
        for (int i = _adCache.Count - 1; i >= 0; --i)
        {
            if (_adCache[i] == null)
            {
                _adCache.RemoveAt(i);
                continue;
            }

            if (_adCache[i].IsError)
            {
                _adCache[i].Release();
            }
        }
        Load();
    }
}

public class AdmobAd
{
    private RewardedAd _rewardedAd = default;

    public bool IsError { get; private set; }
    public bool IsLoaded { get 
        { 
            return _rewardedAd != default; 
        }}
    
    public AdmobAd(string adUnitId)
    {
        Load(adUnitId);
    }
    public void Load(string adUnitId)
    {
        _rewardedAd = default;
        IsError = false;
        // Load a rewarded ad
        RewardedAd.Load(adUnitId, new AdRequest(),
            (RewardedAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    Debug.Log("Rewarded ad failed to load with error: " +
                               loadError.GetMessage());
                    IsError = true;
                    return;
                }
                else if (ad == null)
                {
                    Debug.Log("Rewarded ad failed to load.");
                    IsError = true;
                    return;
                }

                Debug.Log("Rewarded ad loaded.");
                _rewardedAd = ad;
            });
    }
    public void Release()
    {
        _rewardedAd.Destroy();
        _rewardedAd = null;
    }
    public void Show(Action<EResult> callback)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("Rewarded ad granted a reward: " +
                        reward.Amount);
                callback.Invoke(EResult.Success);
            });
        }
        else
        {
            Debug.Log("Rewarded ad cannot be shown.");
            callback.Invoke(EResult.Error);
        }
    }

}
