using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelTouchBlock : MonoBehaviour
{

    [SerializeField] private GameObject blockObject;
    [SerializeField] private GameObject progressObject;


    private void Awake()
    {
        blockObject.SetActive(false);
        progressObject.SetActive(false);
        TouchBlockManager.Instance.RegisterLockPanel(this);
    }

    private void OnDestroy()
    {
        TouchBlockManager.Instance.UnRegisterLockPanel(this);
    }

    public void SetTouchBlock(bool block)
    {
        blockObject.SetActive(block);
    }

    public void SetProgressObject(bool val)
    {
        //progressObject.SetActive(val);
        progressObject.SetActive(false);
    }


}
