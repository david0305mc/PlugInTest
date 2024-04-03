using Google.XR.Cardboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;
using Cysharp.Threading.Tasks;
using System.Threading;

public class VRTest : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject vrCameraRoot;

    private CancellationTokenSource cts;
    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    public void Start()
    {
        mainCamera.gameObject.SetActive(true);
        vrCameraRoot.SetActive(false);

        // Configures the app to not shut down the screen and sets the brightness to maximum.
        // Brightness control is expected to work only in iOS, see:
        // https://docs.unity3d.com/ScriptReference/Screen-brightness.html.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.brightness = 1.0f;
        
        // Checks if the device parameters are stored and scans them if not.
        if (!Api.HasDeviceParams())
        {
            Api.ScanDeviceParams();
        }
    }

    public void OnClickBtnStartXR()
    {
        StartXR().Forget();

    }

    public void OnclickBtnStopXR()
    {
        StopXR().Forget();
    }

    private void CheckEvent()
    {
        if (Api.IsGearButtonPressed)
        {
            Api.ScanDeviceParams();
        }

        if (Api.IsCloseButtonPressed)
        {
            StopXR().Forget();
        }

        if (Api.IsTriggerHeldPressed)
        {
            Api.Recenter();
        }

        if (Api.HasNewDeviceParams())
        {
            Api.ReloadDeviceParams();
        }

#if !UNITY_EDITOR
        Api.UpdateScreenParams();
#endif
    }
    private async UniTask StartXR()
    {
        cts?.Clear();
        cts = new CancellationTokenSource();
        
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        await UniTask.WaitUntil(() => { return Screen.width > Screen.height; }, cancellationToken: cts.Token);
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f), cancellationToken: cts.Token);
        mainCamera.gameObject.SetActive(false);
        vrCameraRoot.SetActive(true);
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f), cancellationToken: cts.Token);
        await XRGeneralSettings.Instance.Manager.InitializeLoader().ToUniTask(cancellationToken: cts.Token);
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: cts.Token);
        }

        while (true)
        {
            await UniTask.Yield(cancellationToken: cts.Token);
            CheckEvent();
        }
    }

    private async UniTaskVoid StopXR()
    {
        cts?.Clear();
        cts = new CancellationTokenSource();
        if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
        
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f), cancellationToken: cts.Token);
        Screen.orientation = ScreenOrientation.Portrait;
        await UniTask.WaitUntil(() => { return Screen.width < Screen.height; }, cancellationToken: cts.Token);
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f), cancellationToken: cts.Token);
        mainCamera.gameObject.SetActive(true);
        mainCamera.ResetAspect();
        vrCameraRoot.SetActive(false);
    }
}
