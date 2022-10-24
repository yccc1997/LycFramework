using System;
using System.Collections.Generic;

/// <summary>
/// 基础事件
/// </summary>
public class BaseEvent
{

    public delegate void Callback(uint uiEvtID, params object[] data);

    /// <summary>
    /// 事件委托类
    /// </summary>
    public class EventDelegate
    {
        private List<Callback> arrCallBack = new List<Callback>();
        private List<Callback> arr2Process = new List<Callback>();

        private uint uiEvtID;
        /// <summary>
        /// 事件ID
        /// </summary>
        public uint EvtID { get { return uiEvtID; } }

        public EventDelegate(uint _uiEvtId)
        {
            uiEvtID = _uiEvtId;
        }

        public void AddCallBack(Callback cb)
        {
            if (!arrCallBack.Contains(cb))
            {
                arrCallBack.Add(cb);
            }
        }

        public void RemoveCallBack(Callback cb)
        {
            arrCallBack.Remove(cb);
        }

        public void ProcEvent(params object[] objData)
        {
            arr2Process.AddRange(arrCallBack);
            for (int i = 0; i < arr2Process.Count; i++)
            {
                Callback cb = arr2Process[i] as Callback;
                cb(uiEvtID, objData);
            }
            arr2Process.Clear();
        }
    }

    public Dictionary<uint, EventDelegate> mDicEvtDelegate = new Dictionary<uint, EventDelegate>();   //事件的集合


    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="uiEvtID"></param>
    /// <param name="callBack"></param>
    public void Reg(uint uiEvtID, Callback callBack)
    {
        EventDelegate evtDelegate = null;
        if (!mDicEvtDelegate.ContainsKey(uiEvtID))
        {
            evtDelegate = new EventDelegate(uiEvtID);
            mDicEvtDelegate.Add(uiEvtID, evtDelegate);
        }
        else
        {
            evtDelegate = mDicEvtDelegate[uiEvtID];
        }
        if (null != evtDelegate)
        {
            evtDelegate.AddCallBack(callBack);
        }
    }

    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="uiEvtID"></param>
    /// <param name="objData"></param>
    /// <returns></returns>
    public void ProcEvent(uint uiEvtID, params object[] objData)
    {
        if (mDicEvtDelegate.ContainsKey(uiEvtID))
        {
            EventDelegate evtDelegate = mDicEvtDelegate[uiEvtID];
            evtDelegate.ProcEvent(objData);
        }
    }

    /// <summary>
    /// 删除事件
    /// </summary>
    /// <param name="uiEvtID"></param>
    /// <param name="callBack"></param>
    public void UnReg(uint uiEvtID, Callback callBack)
    {
        if (mDicEvtDelegate.ContainsKey(uiEvtID))
        {
            EventDelegate evtDelegate = mDicEvtDelegate[uiEvtID];
            evtDelegate.RemoveCallBack(callBack);
        }
    }
    /// <summary>
    /// 删除事件
    /// </summary>
    /// <param name="uiEvtId"></param>
    public void UnReg(uint uiEvtId)
    {
        if (mDicEvtDelegate.ContainsKey(uiEvtId))
        {
            mDicEvtDelegate.Remove(uiEvtId);
        }
    }





    public void Clear(List<uint> avoidList = null)
    {
        if (avoidList == null)
            mDicEvtDelegate.Clear();
        else
        {
            Dictionary<uint, EventDelegate> cache = new Dictionary<uint, EventDelegate>();
            foreach (var i in mDicEvtDelegate)
            {
                if (avoidList.Contains(i.Value.EvtID))
                {
                    cache[i.Key] = i.Value;
                    continue;
                }
            }
            mDicEvtDelegate = cache;
        }
    }
}