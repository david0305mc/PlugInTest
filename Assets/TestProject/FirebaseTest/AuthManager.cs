#if UNITY_ANDROID
#define ENABLE_GOOGLE_PLAY
#elif UNITY_IOS
#define ENABLE_GOOGLE_SIGN
#endif

using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Messaging;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AuthManager : Singleton<AuthManager>, IDisposable
{
    private FirebaseApp _app;
    private FirebaseAuth Auth { get; set; }
    private FirebaseUser User { get; set; }

    private bool initialized = false;
    public UniTaskCompletionSource<string> pushToken = new UniTaskCompletionSource<string>();
    public async UniTask<bool> Initialize()
    {
        if (initialized)
            return false;

#if UNITY_EDITOR
        pushToken.TrySetResult(string.Empty);
#endif      
        var result = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
        if (result == DependencyStatus.Available)
        {
            InitializeFirebase();
            InitializeGPGS();
            if (IsFirebaseSigned() && IsActiveEmptyUser())
            {
                SignOutFirebase();
            }
            initialized = true;
            return true;
        }
        return false;
    }

    public bool IsFirebaseSigned()
    {
        return Auth != null && Auth.CurrentUser != null;
    }

    public EPlatform GetFirebaseSignType()
    {
        if (!IsFirebaseSigned())
            return EPlatform.None;
        if (Auth.CurrentUser.IsAnonymous)
            return EPlatform.Guest;
        foreach (var p in Auth.CurrentUser.ProviderData)
        {
            Debug.LogFormat("[Firebase/ProviderData] {0}", p.ProviderId);
            if (p.ProviderId == GoogleAuthProvider.ProviderId)
                return EPlatform.Google;
            if (p.ProviderId == "apple.com")
                return EPlatform.Apple;
        }
        return EPlatform.Unknown;
    }

    public async UniTask<bool> SignInWithPlatform(EPlatform _platform, CancellationTokenSource _cts)
    {
        if (!IsFirebaseSigned())
        {
            Credential credential;
            switch (_platform)
            {
                case EPlatform.Google:
                    await SignInWithGoogle().AttachExternalCancellation(_cts.Token);
                    break;
                case EPlatform.Guest:
                    await SignInWithGuest().AttachExternalCancellation(_cts.Token);
                    break;
                default:
                    await SignInWithGuest().AttachExternalCancellation(_cts.Token);
                    break;
            }
            Debug.Log($"test1");
        }
        Debug.Log($"test2");
        var token = await User.TokenAsync(true).AsUniTask().AttachExternalCancellation(_cts.Token);
        Debug.Log($"test3 {token}");
        SetActiveUser("Guest");
        token = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImUwM2E2ODg3YWU3ZjNkMTAyNzNjNjRiMDU3ZTY1MzE1MWUyOTBiNzIiLCJ0eXAiOiJKV1QifQ.eyJwcm92aWRlcl9pZCI6ImFub255bW91cyIsImlzcyI6Imh0dHBzOi8vc2VjdXJldG9rZW4uZ29vZ2xlLmNvbS9hYnlzc2NsYXNzaWMiLCJhdWQiOiJhYnlzc2NsYXNzaWMiLCJhdXRoX3RpbWUiOjE3MjY3Mzk0NjgsInVzZXJfaWQiOiJGV213T2MxdTZDWE9mR05Pb1ZlMWlqaGRLVEEzIiwic3ViIjoiRldtd09jMXU2Q1hPZkdOT29WZTFpamhkS1RBMyIsImlhdCI6MTcyNjc0MDAyMSwiZXhwIjoxNzI2NzQzNjIxLCJmaXJlYmFzZSI6eyJpZGVudGl0aWVzIjp7fSwic2lnbl9pbl9wcm92aWRlciI6ImFub255bW91cyJ9fQ.GryMezHmwqb6GmFbQCMEkiqwSs4MTc5uPRieb43PpK-L7poD6r6fAkFrCPYrwDshQLPlpPoAt6zEZMa8r7b36uvgqseoH8yef2AwKZQqbLdKigJVbe1P3BjOW1HP05u87CwHNVNp-Q-5v8Xw2xuVij3VGdu9rE5QZO1A1fqGvOZTo2QGz2zyjZso6y8FFF3w9nlZEMMWHJwfMTgo90F-587SvYkR1MfTf91bqUoEiEnEXmdvI1hqmAhwhYjdCnbFd9U4uIDFBqQ73Mn6BLCTkslrHINzF1nFbR8hw3IQ57bIW7JJYFlsN-yXyfBKzaXI-onOdllpWDZcwMHLO3IXDg";
        var repSignIn = await ServerAPI.SignIn(EPlatform.Guest, token, "KO", string.Empty, default).AttachExternalCancellation(_cts.Token);
        Debug.Log($"test4");
        var repLogin = await ServerAPI.Login(repSignIn.uno, repSignIn.token, default).AttachExternalCancellation(_cts.Token);
        Debug.Log($"test5");
        return true;
    }

    private async UniTask<Credential> GetGoogleCredential()
    {
#if ENABLE_GOOGLE_PLAY
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            PlayGamesPlatform.Instance.SignOut();

        UniTaskCompletionSource ucs = new UniTaskCompletionSource();
        Social.localUser.Authenticate(ret =>
        {
            if (!ret)
            {
                ucs.TrySetCanceled();
                return;
            }

            ucs.TrySetResult();
        });

        await ucs.Task;
        return GoogleAuthProvider.GetCredential(((PlayGamesLocalUser)Social.localUser).GetIdToken(), null);
#endif

#if ENABLE_GOOGLE_SIGN
        var signInUser = await GoogleSignIn.DefaultInstance.SignIn().AsUniTask();
        return GoogleAuthProvider.GetCredential(signInUser.IdToken, null);
#endif
    }
    public async UniTask<Credential> SignInWithGoogle()
    {
        var credential = await GetGoogleCredential();
        await Auth.SignInWithCredentialAsync(credential).AsUniTask();
        return credential;
    }

    private async UniTask<Credential> SignInWithGuest()
    {
        var ret = await Auth.SignInAnonymouslyAsync().AsUniTask();
        return ret.Credential;
    }
    private void InitializeFirebase()
    {
        Debug.Log("[Firebase] Setting up Firebase Auth");
        _app = FirebaseApp.DefaultInstance;
        Auth = FirebaseAuth.DefaultInstance;
        Auth.StateChanged += AuthStateChanged;
        Auth.IdTokenChanged += OnIdTokenChanged;

        //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        FirebaseMessaging.GetTokenAsync().AsUniTask()
            .ContinueWith(x =>
            {
                Debug.LogFormat("[Firebase] FirebaseMessaging Token: {0}", x);
                pushToken.TrySetResult(x);
            }).Forget();
        //AuthStateChanged(this, null);
    }
    void InitializeGPGS()
    {
        var config = new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = BuildSetting.type == EBuildType.Dev;
        PlayGamesPlatform.Activate();
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (Auth.CurrentUser != User)
        {
            bool signedIn = User != Auth.CurrentUser && Auth.CurrentUser != null && Auth.CurrentUser.IsValid();
            if (!signedIn && User != null)
            {
                Debug.Log("Signed out " + User.UserId);
            }
            User = Auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + User.UserId);
                //displayName = user.DisplayName ?? "";
                //emailAddress = user.Email ?? "";
                //photoUrl = user.PhotoUrl ?? "";
            }
        }
    }
    void OnIdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        if (sender == null)
            return;

        Debug.LogFormat("[Firebase/OnIdTokenChanged] Sender : {0}", sender.ToString());
    }

    private void SignOutFirebase()
    {
        Auth.SignOut();
    }

    public void InitUser()
    {
        
    }
    public bool IsActiveEmptyUser()
    {
        return string.IsNullOrEmpty(GetActiveUser());
    }

    public string GetActiveUser()
    {
        string key = string.Format("{0}/DB/User", ServerSetting.serverName);
        return PlayerPrefs.GetString(key);
    }

    public void SetActiveUser(string userName)
    {
        string key = string.Format("{0}/DB/User", ServerSetting.serverName);
        PlayerPrefs.SetString(key, userName);
        
        Debug.LogFormat("[AuthManager/SetActiveUser] {0}", userName ?? "Empty");
    }

    public void Dispose()
    {
        Auth.StateChanged -= AuthStateChanged;
        Auth = null;
    }
}
