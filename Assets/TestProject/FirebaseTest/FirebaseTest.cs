using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.UI;
using TMPro;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Cysharp.Threading.Tasks;

public class FirebaseTest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gpgsText;
    [SerializeField] private TextMeshProUGUI firebaseText;
    private FirebaseAuth auth;
    private FirebaseUser user;

    private void Awake()
    {
        InitializeGPGS();
        InitializeFirebase();
    }

    void InitializeGPGS()
    {
        var config = new PlayGamesClientConfiguration.Builder();
        PlayGamesPlatform.InitializeInstance(
            config.RequestIdToken()
            .RequestEmail()
            .Build());
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    private async UniTask SignInGPGS()
    {
        UniTaskCompletionSource ucs = new UniTaskCompletionSource();
        Social.localUser.Authenticate(ret =>
        {
            if (!ret)
            {
                ucs.TrySetCanceled();
                gpgsText.text = "Failed";
                return;
            }

            ucs.TrySetResult();
            
        });
        await ucs.Task;
        gpgsText.text = $"Sucess {name}";
        StartCoroutine(TryFirebaseLogin()); 
    }
    private void SignOut()
    {
        if (Social.localUser.authenticated)
        {
            PlayGamesPlatform.Instance.SignOut();
            auth.SignOut();
        }
    }
    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                gpgsText.SetText("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                gpgsText.SetText("Signed in " + user.UserId);
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

    IEnumerator TryFirebaseLogin()
    {
        while (string.IsNullOrEmpty(((PlayGamesLocalUser)Social.localUser).GetIdToken()))
            yield return null;
        string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                firebaseText.SetText("SignInWithCredentialAsync was canceled!!");
                return;
            }
            if (task.IsFaulted)
            {
                firebaseText.SetText("SignInWithCredentialAsync encountered an error: " + task.Exception);

                return;
            }
            user = task.Result;

        });
        
        
    }

    public void OnClickBtnGPGSLogin()
    {
        SignInGPGS();
    }
}


