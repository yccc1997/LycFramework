using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ResourceManager : GameMgrBase<ResourceManager>
{
    /// <summary>
    /// 依赖关系字典
    /// </summary>
    public Dictionary<string, DependInfo> DependInfoDic = new Dictionary<string, DependInfo>();
    /// <summary>
    /// 引用计数字典
    /// </summary>
    private Dictionary<string, QuoteCalculate> QuoteCalculateDic = new Dictionary<string, QuoteCalculate>();

    /// <summary>
    /// 正在加载中的资源字典
    /// </summary>
    private Dictionary<string, Resource> loadingLoadResourceDic = new Dictionary<string, Resource>();
    /// <summary>
    /// 完成加载的资源字典
    /// </summary>
    private Dictionary<string, Resource> finishLoadResourceDic = new Dictionary<string, Resource>();
    /// <summary>
    /// 已卸载资源缓存列表
    /// </summary>
    private List<string> unloadCacheList = new List<string>();

    private string RootPath = "file:///D:/Data/Project/LYC/AssetBundlePackage/";

    public void Start()
    {
        InitDependInfoDic();
        InvokeRepeating("ResUninstall", 0, 1);
    }

    #region 初始化依赖字典
    /// <summary>
    /// 初始化依赖字典
    /// </summary>
    public void InitDependInfoDic()
    {
        StartCoroutine(LoadSingleRes("AssetBundlePackage", SetDependDic));
    }

    /// <summary>
    /// 设置依赖字典
    /// </summary>
    /// <param name="res"></param>
    public void SetDependDic(Resource res)
    {
        if (res == null)
        {
            return;
        }
        if (res.assetBundle == null)
        {
            return;
        }
        AssetBundleManifest abm = res.assetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
        foreach (var item in abm.GetAllAssetBundles())
        {
            DependInfo info = new DependInfo();
            info.name = item;
            info.dependList = new List<string>();
            GetDependList(abm, item, ref info.dependList);
            DependInfoDic[item] = info;
        }
    }

    /// <summary>
    /// 得到依赖列表
    /// </summary>
    /// <param name="abm"></param>
    /// <param name="resName"></param>
    /// <param name="nameList"></param>
    private void GetDependList(AssetBundleManifest abm, string resName, ref List<string> nameList)
    {
        if (abm == null || string.IsNullOrEmpty(resName))
        {
            return;
        }
        string[] Dependencies = abm.GetAllDependencies(resName);
        if (Dependencies == null)
        {
            return;
        }
        if (Dependencies.Length != 0)
        {
            for (int i = 0; i < Dependencies.Length; i++)
            {
                if (!nameList.Contains(Dependencies[i]))
                {
                    nameList.Add(Dependencies[i]);
                    GetDependList(abm, Dependencies[i], ref nameList);
                }

            }
        }
    }

    #endregion

    #region 资源加载

    public void Load(string name, ResourcePathType type, Action<Resource> CallBack)
    {
        name = GetPath(name, type);
        StartCoroutine(LoadResource(name, CallBack));
    }
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <param name="resName">资源名称</param>
    /// <param name="CallBack">加载回调</param>
    /// <returns></returns>
    public IEnumerator LoadResource(string resName, Action<Resource> CallBack)
    {
        Resource res;
        if (finishLoadResourceDic.TryGetValue(resName, out res))
        {
            ResourceManager.Instance.AddQuoteCalculate(resName, true);
            res.AddCallBack(CallBack);
        }
        else
        {
            DependInfo dependInfo;
            while (DependInfoDic==null || DependInfoDic.Count==0)
            {
                yield return null;
            }
            if (DependInfoDic.TryGetValue(resName, out dependInfo))
            {
                if (dependInfo.dependList != null)
                {
                    for (int i = 0; i < dependInfo.dependList.Count; i++)
                    {
                        yield return LoadSingleRes(dependInfo.dependList[i], null);
                    }
                }
                yield return LoadSingleRes(resName, CallBack);
            }
        }
    }

    /// <summary>
    /// 加载单一资源
    /// </summary>
    /// <param name="resName">资源名称</param>
    /// <param name="CallBack">资源回调</param>
    /// <returns></returns>
    public IEnumerator LoadSingleRes(string resName, Action<Resource> CallBack)
    {

        //如果此资源已经加载完成直接调用已经加载完毕的资源回调
        if (finishLoadResourceDic.ContainsKey(resName))
        {
            finishLoadResourceDic[resName].AddCallBack(CallBack);
            yield return null;
        }
        else
        {
            if (!loadingLoadResourceDic.ContainsKey(resName))
            {
                string name = resName;
                string path = RootPath + name;
                Resource dependRes = new Resource(resName, CallBack);
                loadingLoadResourceDic.Add(name, dependRes);
                yield return dependRes.GetData(path);
                loadingLoadResourceDic.Remove(name);
                finishLoadResourceDic[name] = dependRes;
            }
            else
            {
                Resource dependRes = loadingLoadResourceDic[resName];
                if (dependRes != null)
                {
                    loadingLoadResourceDic[resName].AddCallBack(CallBack);
                }
                while (loadingLoadResourceDic.ContainsKey(resName))
                {
                    yield return null;
                }

            }
        }

        ResourceManager.Instance.AddQuoteCalculate(resName, false);
    }

    /// <summary>
    /// 依赖资源是否加载完毕
    /// </summary>
    /// <param name="name"></param>
    public bool IsLoadFinshDepend(string name)
    {
        if (DependInfoDic.ContainsKey(name))
        {
            DependInfo info = DependInfoDic[name];
            if (info != null && info.dependList != null)
            {
                int length = info.dependList.Count;
                for (int i = 0; i < length; i++)
                {
                    if (!finishLoadResourceDic.ContainsKey(info.dependList[i]))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    #endregion

    #region 资源卸载
    public void ResUninstall()
    {

        if (QuoteCalculateDic.Count == 0)
        {
            return;
        }
        unloadCacheList.Clear();
        foreach (var item in QuoteCalculateDic)
        {
            if (item.Value != null)
            {
                if (item.Value.QuoteNumber == 0)
                {
                    item.Value.resRemoveCountdown--;
                    if (item.Value.resRemoveCountdown <= 0)
                    {
                        if (finishLoadResourceDic.ContainsKey(item.Key))
                        {
                            finishLoadResourceDic[item.Key].assetBundle.Unload(true);
                            finishLoadResourceDic.Remove(item.Key);
                            unloadCacheList.Add(item.Key);
                        }

                    }
                }

            }
        }
        if (unloadCacheList.Count != 0)
        {
            for (int i = 0; i < unloadCacheList.Count; i++)
            {
                if (QuoteCalculateDic.ContainsKey(unloadCacheList[i]))
                {
                    QuoteCalculateDic.Remove(unloadCacheList[i]);
                }

            }
        }
    }

    /// <summary>
    /// 添加引用计数
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="IsAddDependName">是否添加依赖资源计数</param>
    public void AddQuoteCalculate(string name, bool IsAddDependName)
    {
        AddQuoteCalculate(name);
        if (IsAddDependName)
        {
            if (DependInfoDic.ContainsKey(name))
            {
                DependInfo info = DependInfoDic[name];
                if (info != null && info.dependList != null)
                {
                    int length = info.dependList.Count;
                    for (int i = 0; i < length; i++)
                    {
                        AddQuoteCalculate(info.dependList[i]);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 移除资源计数
    /// </summary>
    /// <param name="name">资源名称完整路径</param>
    /// <param name="IsAddDependName">是否移除依赖资源计数</param>
    public void RemoveQuoteCalculate(string name, bool IsRemoveDependName)
    {
        RemoveQuoteCalculate(name);
        if (IsRemoveDependName)
        {
            DependInfo info = DependInfoDic[name];
            if (info != null && info.dependList != null)
            {
                int length = info.dependList.Count;
                for (int i = 0; i < length; i++)
                {
                    RemoveQuoteCalculate(info.dependList[i]);
                }
            }
        }
    }
    /// <summary>
    /// 移除资源计数
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="name">资源类型</param>
    /// <param name="IsAddDependName">是否移除依赖资源计数</param>
    public void RemoveQuoteCalculate(string name,ResourcePathType type, bool IsRemoveDependName)
    {
        name = GetPath(name, type);
        RemoveQuoteCalculate(name, IsRemoveDependName);
    }

    /// <summary>
    /// 添加引用计数
    /// </summary>
    /// <param name="name">AB资源名称</param>
    private void AddQuoteCalculate(string name)
    {
        if (QuoteCalculateDic == null)
        {
            QuoteCalculateDic = new Dictionary<string, QuoteCalculate>();
        }
        if (!QuoteCalculateDic.ContainsKey(name))
        {
            QuoteCalculateDic.Add(name, new QuoteCalculate());
        }
        QuoteCalculateDic[name].QuoteNumber++;
    }

    /// <summary>
    /// 移除资源计数
    /// </summary>
    private void RemoveQuoteCalculate(string name)
    {
        if (QuoteCalculateDic == null)
        {
            QuoteCalculateDic = new Dictionary<string, QuoteCalculate>();
        }
        if (QuoteCalculateDic.ContainsKey(name))
        {
            QuoteCalculateDic[name].QuoteNumber--;
        }
    }

    #endregion

    #region 得到文件路径
    private string GetPath(string name, ResourcePathType resType = ResourcePathType.None)
    {
        switch (resType)
        {
            case ResourcePathType.None:
                return name;
            case ResourcePathType.Material:
                return "assetbundleres/material/" + name;
            case ResourcePathType.Prefab:
                return "assetbundleres/prefab/" + name;
            case ResourcePathType.Shader:
                return "assetbundleres/shader/" + name;
            case ResourcePathType.Texture:
                return "assetbundleres/texture/" + name;
            case ResourcePathType.UI:
                return "assetbundleres/prefab/ui/" + name;
            case ResourcePathType.Model:
                return "assetbundleres/model/" + name;
            case ResourcePathType.Other:
                return "assetbundleres/other/" + name;
            default:
                return name;
        }
    }

    #endregion

}

/// <summary>
/// 依赖信息
/// </summary>
public class DependInfo
{
    //资源名称
    public string name;
    /// <summary>
    /// 资源依赖的所有资源列表
    /// </summary>
    public List<string> dependList;
}

/// <summary>
/// 引用计数
/// </summary>
public class QuoteCalculate
{
    public string resName;

    private int quoteNumber;
    /// <summary>
    /// 引用次数
    /// </summary>
    public int QuoteNumber
    {
        get
        {
            return quoteNumber;
        }

        set
        {
            quoteNumber = value;
            resRemoveCountdown = 30;
        }
    }

    public int resRemoveCountdown = 30;
}


