using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleModel
{
    public GameObject modelRoot;
    public AnimatorCtrl anim;
    public GameObject modelObj;
    public RoleData roleData;
    public RoleModel(RoleData Data)
    {
        roleData = Data;
        if (roleData == null)
        {
            return;
        }
        Init(roleData);
    }
    private void Init(RoleData Data)
    {
        modelRoot = new GameObject(Data.Id.ToString());
        ModelLoad(Data.ModelPath);
    }
    public void ModelLoad(string modelName)
    {
        Debug.Log(modelName);
        ResourceManager.Instance.Load(modelName, ResourcePathType.Other, ModelLoadCallback);
    }

    public void ModelLoadCallback(Resource res)
    {
        if (modelRoot == null)
        {
            return;
        }
        GameObject obj = res.LoadGameObjectFinsh(modelRoot.transform);
        modelObj = obj;
        anim = new AnimatorCtrl(obj);
    }


}
