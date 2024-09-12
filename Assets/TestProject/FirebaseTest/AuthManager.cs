using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Messaging;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthManager : Singleton<AuthManager>
{
    private FirebaseAuth auth;
    private FirebaseUser user;

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
            initialized = true;
            return true;
        }
        return false;
    }

    public async UniTask<bool> SignIn()
    {
        UniTaskCompletionSource<bool> ucs = new UniTaskCompletionSource<bool>();
        Social.localUser.Authenticate(ret =>
        {
            ucs.TrySetResult(ret);
        });

        if (!await ucs.Task)
        {
            return false;
        }

        var token = await SignInWithGoogle();
        return true;
    }

    private async UniTask<string> SignInWithGoogle()
    {
        while (string.IsNullOrEmpty(((PlayGamesLocalUser)Social.localUser).GetIdToken()))
            await UniTask.Yield();

        string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        user = await auth.SignInWithCredentialAsync(credential).AsUniTask();

        return await user.TokenAsync(false).AsUniTask();
    }

    private void InitializeFirebase()
    {
        Debug.Log("[Firebase] Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += OnIdTokenChanged;
        //FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        FirebaseMessaging.GetTokenAsync().AsUniTask()
            .ContinueWith(x =>
            {
                Debug.LogFormat("[Firebase] FirebaseMessaging Token: {0}", x);
                pushToken.TrySetResult(x);
            }).Forget();
        AuthStateChanged(this, null);
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
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                //displayName = user.DisplayName ?? "";
                //emailAddress = user.Email ?? "";
                //photoUrl = user.PhotoUrl ?? "";
            }
        }
    }
    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
    void OnIdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        if (sender == null)
            return;

        Debug.LogFormat("[Firebase/OnIdTokenChanged] Sender : {0}", sender.ToString());
    }


}
