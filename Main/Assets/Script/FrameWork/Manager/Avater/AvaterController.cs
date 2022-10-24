using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvaterController : GameMgrBase<AvaterController>
{
    protected override void Init()
    {
        base.Init();
        RegistMessages();
    }
    public void RegistMessages()
    {
        EventManager.Instance.Reg_Client(EEvent.PlayerMove, RegPlayerMove);
    }
    public void RegPlayerMove(uint id, params object[] data)
    {
        if (data==null)
        {
            return;
        }
        int dataid = (int)data[0];
        EDirection dir = (EDirection)data[1];

        Role nowRole;
        if (AvaterManager.Instance.RoleDic.TryGetValue(dataid, out nowRole))
        {
            nowRole.roleData.Move(dir);
        }  
    }
}
