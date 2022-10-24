using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BaseEvent;

public class EventManager : GameMgrBase<EventManager>
{
    private BaseEvent clientEvent;

    public BaseEvent ClientEvent
    {
        get
        {
            if (clientEvent == null)
            {
                clientEvent = new BaseEvent();
            }
            return clientEvent;
        }
        set => clientEvent = value;
    }

    /// <summary>
    /// 添加事客户端件
    /// </summary>
    /// <param name="uiEvtID"></param>
    /// <param name="callBack"></param>
    public void Reg_Client(EEvent uiEvtID, Callback callBack)
    {
         
        ClientEvent.Reg((uint)uiEvtID, callBack);
    }
    /// <summary>
    /// 发布客户端事件
    /// </summary>
    /// <param name="uiEvtID"></param>
    /// <param name="callBack"></param>
    public void Proc_Client(EEvent uiEvtID, params object[] objData)
    {
        ClientEvent.ProcEvent((uint)uiEvtID, objData);
    }
}
