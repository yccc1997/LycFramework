using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectExtraneousInfo : MonoBehaviour
{
    /// <summary>
    /// 资源名称
    /// </summary>
    private string resName;

    public void Init(string resName)
    {
        this.resName = resName;
    }

    private void OnDestroy()
    {
        ResourceManager.Instance.RemoveQuoteCalculate(resName, true);
    }




}
