using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoginPanel : UIBase
{
    public Button button;
    public void OnClick()
    {
        UIManager.Instance.ClosePanel("UILoginPanel");
        UIManager.Instance.CreatPanel("UIRolePanel");

    }
}
