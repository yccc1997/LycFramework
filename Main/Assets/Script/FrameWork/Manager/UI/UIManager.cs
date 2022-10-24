using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : GameMgrBase<UIManager>
{
    /// <summary>
    /// 所有面板字典
    /// </summary>
    private Dictionary<string, UIBase> allPanelDic;
    /// <summary>
    /// 开启的面板字典
    /// </summary>
    private Dictionary<string, UIBase> openPanelDic;
    /// <summary>
    /// 隐藏面板字典
    /// </summary>
    private Dictionary<string, UIBase> hidePanelDic;

    /// <summary>
    /// 等待开启的面板
    /// </summary>
    private List<string> waitOpenPanelList;

    private GameObject uiRoot;
    /// <summary>
    /// 面板跟节点
    /// </summary>
    public GameObject UIRoot
    {
        get
        {
            if (uiRoot==null)
            {
                uiRoot = GameObject.Find("Canvas/UIRoot");
            }
            return uiRoot;
        }
    }

    protected override void Init()
    {
        allPanelDic = new Dictionary<string, UIBase>();
        openPanelDic = new Dictionary<string, UIBase>();
        hidePanelDic = new Dictionary<string, UIBase>();
        waitOpenPanelList = new List<string>();
    }
    #region 公开

    /// <summary>
    /// 创建面板
    /// </summary>
    public void CreatPanel(string panelName)
    {
        panelName = panelName.ToLower();
        if (openPanelDic.ContainsKey(panelName))
        {
            return;
        }
        if (waitOpenPanelList.Contains(panelName))
        {
            return;
        }
        waitOpenPanelList.Add(panelName);
        ResourceManager.Instance.Load(panelName,ResourcePathType.UI, CreatPanelCallback);
    }

    /// <summary>
    /// 生成面板回调
    /// </summary>
    /// <param name="res"></param>
    public void CreatPanelCallback(Resource res)
    {
        if (res == null) return;
        if (res.MirrorObj == null) return;
        if (UIRoot==null) return;
        GameObject obj = res.LoadGameObjectFinsh(UIRoot.transform);
        UIBase uiBase = obj.GetComponent<UIBase>();
     
        if (uiBase == null)
        {
            Destroy(obj);
            ResourceManager.Instance.RemoveQuoteCalculate(res.DataName,ResourcePathType.UI, true);
        }
        else
        {
            uiBase.res = res;
            openPanelDic.Add(res.DataName, uiBase);
        }
        if (waitOpenPanelList.Contains(res.DataName))
        {
            waitOpenPanelList.Remove(res.DataName);
        }
    }
    /// <summary>
    /// 关闭面板
    /// </summary>
    public void ClosePanel(string panelName)
    {
        panelName = panelName.ToLower();
        if (!openPanelDic.ContainsKey(panelName))
        {
            return;
        }
        UIBase uiBase = openPanelDic[panelName];
        if (uiBase==null)
        {
            return;
        }
        Destroy(uiBase.gameObject);
        openPanelDic.Remove(panelName);
        ResourceManager.Instance.RemoveQuoteCalculate(panelName,ResourcePathType.UI, true);
    }

    #endregion

    #region 私有

    private void LoadPanel()
    { 
    
    }

    #endregion
}
