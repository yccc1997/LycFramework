using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        Init();
    }
    private void Init()
    {
        ResourceManager.CreateInstance(this.transform);
        AvaterManager.CreateInstance(this.transform);
        AvaterController.CreateInstance(this.transform);
        UIManager.CreateInstance(this.transform);
        LuaManager.CreateInstance(this.transform);
        EventManager.CreateInstance(this.transform);

    }
}
