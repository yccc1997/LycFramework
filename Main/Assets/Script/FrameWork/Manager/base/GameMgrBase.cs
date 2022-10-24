using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgrBase<T> : MonoBehaviour where T: MonoBehaviour             
{
    private static T instance;
    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    private void Start()
    {   
        Init();
    }

    protected virtual void Init() { }

    public static void CreateInstance(Transform parent)
    {
        if (instance != default(T))
        {
            return;
        }
        GameObject go = new GameObject(typeof(T).ToString());
        if (parent != null)
        {
            go.transform.parent = parent;
        }
        instance = go.AddComponent<T>();
    }
}
