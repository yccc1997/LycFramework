using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvaterManager : GameMgrBase<AvaterManager>
{
    private Dictionary<int, Role> roleDic;
    /// <summary>
    /// 玩家列表字典
    /// </summary>
    public Dictionary<int, Role> RoleDic
    {
        get
        {                                                                                   
            if (roleDic == null)
            {
                roleDic = new Dictionary<int, Role>();
            }
            return roleDic;
        }
        set => roleDic = value;
    }

    private Me me;
    public Me Me { get => me; set => me = value; }

    protected override void Init()
    {
        base.Init();
        InitMainPlayer();
    }

    private void InitMainPlayer()
    {
        RoleData data = new RoleData(1, "eri/prefab/eri_schooluniform");
        Me = new Me(data);
        AddAvater(1, Me);
    }


    private void AddAvater(int id, Role role)
    {
        if (RoleDic.ContainsKey(id))
        {
            RoleDic[id] = role;
        }
        else
        {
            RoleDic.Add(id, role);
        }
    }
    private void RemoveAvater(int id)
    {
        if (RoleDic.ContainsKey(id))
        {
            RoleDic[id].Destory();
            RoleDic[id] = null;
        }
    }
    private void DestoryAll()
    {
        foreach (var item in RoleDic)
        {
            item.Value.Destory();
        }
        RoleDic = null;
    }

    public void Update()
    {
        foreach (var item in RoleDic)
        {
            item.Value.Update();
        }
    }
}
