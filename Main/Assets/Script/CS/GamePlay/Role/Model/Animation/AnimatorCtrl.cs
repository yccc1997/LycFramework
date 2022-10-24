using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorCtrl 
{
    public Animator animator;
    public AnimatorCtrl(GameObject obj)
    {
        Init(obj);
    }

    public void Init(GameObject obj)
    {
        if (obj==null)
        {
            return;
        }
        animator = obj.GetComponent<Animator>();
    }
    /// <summary>
    /// 播放动画
    /// </summary>
    public void Play()
    {
       
    }
}
