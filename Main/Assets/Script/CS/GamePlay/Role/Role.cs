using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Role
{
    public RoleData roleData;
    public RoleModel model;
    public Role(RoleData roleData)
    {
        Init(roleData);
    }



    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(RoleData Data)
    {
        roleData = Data;
        if (roleData==null)
        {
            return;
        }
        InitModel();
    }
    /// <summary>
    /// 初始化模型
    /// </summary>
    public void InitModel()
    {
        model = new RoleModel(roleData);
    }

    /// <summary>
    /// 更新
    /// </summary>
    public void Update()
    {
        RefreshModel();
    }
    /// <summary>
    /// 销毁
    /// </summary>
    public void Destory()
    {

    }

    public void RefreshModel()
    {
        RefreshPos();
        RefershRotation();
        RefreshAnim();
    }

    /// <summary>
    /// 同步模型位置
    /// </summary>
    public void RefreshPos()
    {
        if (model == null || model.modelObj == null || roleData == null)
        {
            return;
        }
        if (model.modelRoot.transform.localPosition != roleData.NowCell)
        {
            model.anim.animator.SetInteger("animation", 10);
        }
        else
        {
            model.anim.animator.SetInteger("animation", 1);
        }
        Vector3 localpos = model.modelRoot.transform.localPosition;
        Vector3 localDir = model.modelRoot.transform.localEulerAngles;
        model.modelRoot.transform.localPosition = Vector3.MoveTowards(localpos, roleData.NowCell, 0.5f);
        model.modelRoot.transform.localEulerAngles = Vector3.MoveTowards(localDir, roleData.Direction_v3, 10f);
      
       /// model.anim.animator.SetInteger("animation", 10);
        //   model.modelObj.transform.localRotation = roleData.NextCell;
    }
    /// <summary>
    /// 同步旋转
    /// </summary>
    public void RefershRotation()
    {

    }
    /// <summary>
    /// 刷新模型动作
    /// </summary>
    public void RefreshAnim()
    {

    }
}
