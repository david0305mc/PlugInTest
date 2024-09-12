using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class FirebaseTest2 : MonoBehaviour
{
    [SerializeField] private Button loginButton;

    private void Awake()
    {
        loginButton.onClick.AddListener(() =>
        {
            StartGame().Forget();
        });
        loginButton.gameObject.SetActive(false);
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
        var serverStatus = ServerAPI.GetServerStatus(default);
        loginButton.gameObject.SetActive(false);
        bool success = await AuthManager.Instance.SignIn();
        if (!success)
        {
            loginButton.gameObject.SetActive(true);
            //Debug.Log("AuthenticatePlatform success");
            //string authToken = await AuthManager.Instance.SignInWithGoogle();
            //Debug.Log($"authToken {authToken}");
        }
    }
}
