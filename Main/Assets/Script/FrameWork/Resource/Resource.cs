using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Resource
{
    public UnityEngine.Object MirrorObj;
    private byte[] MirrorBytes;

    /// <summary>
    /// 资源完整名称（完整路径）
    /// </summary>
    public string resName;
    /// <summary>
    /// 资源名称
    /// </summary>
    private string dataName;
    public string DataName
    {
        get
        {
            if (string.IsNullOrEmpty(dataName))
            {
                dataName = resName.Substring(resName.LastIndexOf('/')+1);
            }
            return dataName;
        }
    }
    /// <summary>
    /// 资源路径
    /// </summary>
    public string dataPath;
    public string DataPath
    {
        get
        {
            if (string.IsNullOrEmpty(dataPath))
            {
                dataPath = "";
            }
            return dataPath;
        }
    }
    public AssetBundle assetBundle;
    private DependInfo dependInfo;
    private bool IsLoadFinsh = false;

    private ResourceType loaclType;
    public ResourceType LoaclType
    {
        get
        {
            return loaclType;
        }

        set
        {
            loaclType = value;
        }
    }
    public Action<Resource> CallBack;
    public Resource(string name, Action<Resource> CallBack)
    {
        this.resName = name;
        this.CallBack += CallBack;
        this.LoaclType = ResourceType.GameObject;
        this.IsLoadFinsh = false;
    }
    public void AddCallBack(Action<Resource> CallBack)
    {
        this.CallBack += CallBack;
        if (ResourceManager.Instance.IsLoadFinshDepend(resName))
        {
            LoadFinish();
        }
    }


    #region 资源加载
    /// <summary>
    /// 得到资源
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerator GetData(string path)
    {

        switch (LoaclType)
        {
            case ResourceType.Bytes:
                using (UnityWebRequest www = UnityWebRequest.Get(path))
                {
                    yield return www.SendWebRequest();
                    if (www.isNetworkError || www.isHttpError)
                    {
                        LoadError();
                    }
                    else
                    {
                        MirrorBytes = www.downloadHandler.data;
                        IsLoadFinsh = true;
                        LoadFinish();
                    }
                }
                break;
            case ResourceType.GameObject:
                using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(path))
                {
                    yield return www.SendWebRequest();
                    try
                    {
                        assetBundle = DownloadHandlerAssetBundle.GetContent(www);
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.Log(ex);
                        LoadError();
                    }
                    yield return SyncLoadMainAsset();
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 加载资源
    /// </summary>
    /// <returns></returns>
    public IEnumerator SyncLoadMainAsset()
    {
        if (assetBundle == null)
        {
            yield break;
        }
        string[] assetNameS = assetBundle.GetAllAssetNames();
        //如果AB的名称为空的话加载资源失败
        if (assetNameS == null || assetNameS.Length == 0)
        {
            LoadError();
            yield break;
        }

        string assetName = assetNameS[0];
        //同步加载
        MirrorObj = assetBundle.LoadAsset(assetName);
        yield return null;
        IsLoadFinsh = true;
        LoadFinish();

    }
    #endregion

    #region 加载成功 资源处理
    public void LoadFinish()
    {
        if (IsLoadFinsh == false)
        {
            return;
        }
        if (CallBack != null)
        {
            CallBack(this);
            CallBack = null;
        }
    }

    public GameObject LoadGameObjectFinsh(Transform parent = null)
    {
        GameObject obj = UnityEngine.Object.Instantiate(MirrorObj, parent) as GameObject;
        return obj;
    }

    #endregion

    private void LoadError()
    {

    }

}
public enum ResourceType
{
    /// <summary>
    /// 二进制数据
    /// </summary>
    Bytes,
    /// <summary>
    /// 游戏对象
    /// </summary>
    GameObject,

}


