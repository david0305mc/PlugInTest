using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPlatformPopup : MonoBehaviour
{
    [SerializeField] private Button googleButton;
    [SerializeField] private Button appleButton;
    [SerializeField] private Button guestButton;

    private System.Action<EPlatform> callback;

    private void Awake()
    {
        googleButton.onClick.AddListener(() =>
        {
            callback.Invoke(EPlatform.Google);
        });
        appleButton.onClick.AddListener(() =>
        {
            callback.Invoke(EPlatform.Apple);
        });
        guestButton.onClick.AddListener(() =>
        {
            callback.Invoke(EPlatform.Guest);
        });
    }
    public void Set(System.Action<EPlatform> _callback)
    {
        callback = _callback;
    }
}
