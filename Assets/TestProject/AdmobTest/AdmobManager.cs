using GoogleMobileAds.Api;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

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


    private bool _initialized = false;
    private List<AdmobUnit> _adUnits = new List<AdmobUnit>();

    public void Initialize()
    {
//#if UNITY_EDITOR
//        _initialized = true;
//#endif
        if (_initialized)
            return;

        Debug.Log("[Admob] Try Initialize..");
        MobileAds.Initialize(_ =>
        {
            Debug.Log("[Admob] MobileAds.Initialized");

            //[Todo] 미디에이션 설정
            //AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
            _adUnits.Add(new AdmobUnit(adUnitId));
            RegistTestDevice();
        });

        _initialized = true;
    }

    public void RegistTestDevice()
    {
        RequestConfiguration requestConfiguration = MobileAds.GetRequestConfiguration()
            .ToBuilder()
            .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified).SetTestDeviceIds(new List<string>() {
                "03266CD6F563177473AF491D46F688C8",
            }).build();
        MobileAds.SetRequestConfiguration(requestConfiguration);
        //requestConfiguration.TestDeviceIds.Add("2077ef9a63d2b398840261c8221a0c9b");
    }

    public UniTask<EResult> Show(int idx = 0)
    {

//#if UNITY_EDITOR
//        return TestShow();
//#endif
        return _adUnits[idx].Show();
    }

#if UNITY_EDITOR
    public async UniTask<EResult> TestShow()
    {
        
        Debug.LogFormat("[Admob] TestShow...");
        await UniTask.Delay(TimeSpan.FromSeconds(1.0));

        return EResult.Success;
    }
#endif
}


public class AdmobUnit
{
    private string _unitId;
    private const int _cacheSize = 3;
    private List<AdmobAd> _adCache = new List<AdmobAd>();

    public AdmobUnit(string id)
    {
        _unitId = id;
        Load();
    }

    void Load()
    {
        while (_adCache.Count < _cacheSize)
            Create();
    }

    void Create()
    {
        var req = new AdRequest.Builder()
            .AddExtra("max_ad_content_rating", "MA")
            .Build();

        _adCache.Add(new AdmobAd(req, _unitId));
    }

    void UpdateCache()
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
                _adCache.RemoveAt(i);
            }
        }

        Load();
    }

    void Remove(AdmobAd ad)
    {
        ad.Release();
        _adCache.Remove(ad);
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

    public async UniTask<EResult> Show()
    {
        try
        {
            AdmobAd ad = await Get();
            if (ad == null)
                return EResult.Error;

            ad.Show();
            await UniTask.WaitUntil(() => ad.IsClosed || ad.IsError);
            if (ad.IsError)
            {
                Remove(ad);
                return EResult.Error;
            }

            //Complete 가 Close 뒤에 호출되는 예외적인 상황으로 대기
            await UniTask.Delay(TimeSpan.FromSeconds(0.2));
            Remove(ad);

            return ad.IsComplete ? EResult.Success : EResult.None;
        }
        finally
        {
            UpdateCache();
        }
    }
}


public class AdmobAd
{
    private RewardedAd _data;

    public bool IsLoaded { get; private set; }
    public bool IsClosed { get; private set; }
    public bool IsError { get; private set; }
    public bool IsComplete { get; private set; }

    public AdmobAd(AdRequest request, string adUnitId)
    {
        _data = new RewardedAd(adUnitId);
        _data.OnAdLoaded += OnAdLoaded;
        _data.OnAdFailedToLoad += OnAdFailedToLoad;
        _data.OnAdOpening += OnAdOpening;
        _data.OnAdFailedToShow += OnAdFailedToShow;
        _data.OnAdClosed += OnAdClosed;
        _data.OnUserEarnedReward += OnUserEarnedReward;
        _data.LoadAd(request);
    }

    public void Release()
    {
        _data.OnAdLoaded -= OnAdLoaded;
        _data.OnAdFailedToLoad -= OnAdFailedToLoad;
        _data.OnAdOpening -= OnAdOpening;
        _data.OnAdFailedToShow -= OnAdFailedToShow;
        _data.OnAdClosed -= OnAdClosed;
        _data.OnUserEarnedReward -= OnUserEarnedReward;
        _data.Destroy();
        _data = null;
    }

    public void Show()
    {
        _data.Show();
    }

    private void OnAdLoaded(object sender, EventArgs args)
    {
        IsLoaded = true;
        Debug.Log("[Admob] OnAdLoaded " + ((RewardedAd)sender).GetResponseInfo().GetMediationAdapterClassName());
    }

    private void OnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        IsError = true;
        Debug.Log("[Admob] OnAdFailedToLoad " + args.LoadAdError);
    }

    private void OnAdOpening(object sender, EventArgs args) { }

    private void OnAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        Debug.Log("[Admob] OnAdFailedToShow " + args.AdError.GetMessage());

        IsClosed = true;
        IsError = true;
    }

    private void OnAdClosed(object sender, EventArgs args)
    {
        Debug.Log("[Admob] OnAdClosed");
        IsClosed = true;
    }

    private void OnUserEarnedReward(object sender, Reward args)
    {
        Debug.Log("[Admob] OnUserEarnedReward");
        IsComplete = true;
    }
}

public enum EResult : int
{
    None,
    Success,
    Error,
}