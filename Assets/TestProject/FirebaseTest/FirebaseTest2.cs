using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Threading;
using TMPro;
using UniRx;

public class FirebaseTest2 : MonoBehaviour
{
    [SerializeField] private SelectPlatformPopup platformPopup;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button levelOutButton;
    [SerializeField] private Button connectGoogleButton;
    [SerializeField] private Button connectAppleButton;
    [SerializeField] private GameObject mainObj;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI unoText;


    private CompositeDisposable compositeDisposable = new CompositeDisposable();
    private CancellationTokenSource cancelltaionTokenSource = new CancellationTokenSource();
    private void Awake()
    {
        mainObj.SetActive(false);
        platformPopup.gameObject.SetActive(false);
        loginButton.onClick.AddListener(() =>
        {
            StartGame().Forget();
        });
        levelUpButton.onClick.AddListener(() =>
        {
            //UserDataManager.Instance.baseData.level++;
            UserDataManager.Instance.baseData.AddGold();
            //UserDataManager.Instance.baseData.AddDicTest(1);
            //UserDataManager.Instance.inventoryData.AddItem();
            ServerAPI.SaveToServer();

        });
        levelOutButton.onClick.AddListener(() =>
        {
            AuthManager.Instance.SignOut();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

        });

        loginButton.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        cancelltaionTokenSource.Clear();
        AuthManager.Instance.Dispose();
        compositeDisposable.Clear();

    }

    void Start()
    {
        Init().Forget();   
    }

    private async UniTaskVoid Init()
    {
        Debug.Log("Init Try");
        var result = await AuthManager.Instance.Initialize();
        if (result)
        {
            Debug.Log("Init Success");
            loginButton.gameObject.SetActive(true);
        }
    }
    private async UniTaskVoid StartGame()
    {
        Debug.Log("try SignIn");
        var serverStatus = await ServerAPI.GetServerStatus(cancellationToken: cancelltaionTokenSource.Token);
        loginButton.gameObject.SetActive(false);
        platformPopup.gameObject.SetActive(true);
        UniTaskCompletionSource<EPlatform> ucs = new UniTaskCompletionSource<EPlatform>();
        platformPopup.Set(_platform =>
        {
            ucs.TrySetResult(_platform);
        });
        var platform = await ucs.Task;
#if UNITY_EDITOR
        platform = EPlatform.Guest;
#endif
        platformPopup.gameObject.SetActive(false);
        mainObj.SetActive(true);

        try
        {
            await AuthManager.Instance.SignInWithPlatform(platform, cancelltaionTokenSource);
            unoText.SetText(UserDataManager.Instance.Uno.ToString());
        }
        catch
        {
            Debug.LogError("Error");
        }

        UserDataManager.Instance.baseData.gold.Subscribe(_gold =>
        {
            levelText.SetText(_gold.ToString());
        }).AddTo(compositeDisposable);


        //Debug.Log("AuthenticatePlatform success");
        //string authToken = await AuthManager.Instance.SignInWithGoogle();
        //Debug.Log($"authToken {authToken}");
    }
}
