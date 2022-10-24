using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Text_yc : MonoBehaviour
{

    string url = "file:///D:/Data/Project/LYC/AssetBundlePackage/";
    public string name = "";

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            // ResourceManager.Instance.Load(name,ResourcePathType.Prefab, Callback1);
            //   UIManager.Instance.CreatPanel(name);
            // Role role = new Role("eri/prefab/eri_schooluniform");
            // AvaterManager.Instance.Me.roleData.NowCell += Vector3.right*0.01f;
            EventManager.Instance.Proc_Client(EEvent.PlayerMove, 1, EDirection.Forward);
        }
        if (Input.GetKey(KeyCode.S))
        {
            // ResourceManager.Instance.Load(name,ResourcePathType.Prefab, Callback1);
            //UIManager.Instance.ClosePanel(name);

          //  AvaterManager.Instance.Me.roleData.NowCell += Vector3.right * 10f;
            EventManager.Instance.Proc_Client(EEvent.PlayerMove, 1, EDirection.Backward);
        }

        if (Input.GetKey(KeyCode.D))
        {
            EventManager.Instance.Proc_Client(EEvent.PlayerMove, 1, EDirection.Right);
        }
        if (Input.GetKey(KeyCode.A))
        {
            EventManager.Instance.Proc_Client(EEvent.PlayerMove, 1, EDirection.Left);
        }
    }

    public void Callback1(Resource res)
    {
        if (res.MirrorObj == null) return;
        GameObject obj = res.LoadGameObjectFinsh(this.gameObject.transform);
    }
    public void Callback2(Resource res)
    {
        if (res.MirrorObj == null) return;
        GameObject obj = res.LoadGameObjectFinsh(this.gameObject.transform);
    }
}
