using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimEditor : MonoBehaviour
{
    [MenuItem("lyc/添加动画片段")]
    public static void AddClipText()
    {
        AnimatorController anim = Selection.activeObject as AnimatorController;
        anim.AddParameter("LYCtext", AnimatorControllerParameterType.Int);
        AnimatorControllerParameter[] apS=anim.parameters;
        for (int i = 0; i < apS.Length; i++)
        {
            anim.RemoveParameter(apS[i]);
        }
        InitParameterData();
        AddParameter(anim);

        string[] str = Directory.GetFiles(@"G:\lycGitHub\LycFramework\Main\Assets\AssetBundleRes\other\Animations");
        Vector3 point = new Vector3();
        int index = 0;
        for (int i = 0; i < str.Length; i++)
        {
            string path = str[i].Replace("G:\\lycGitHub\\LycFramework\\Main\\Assets", "Assets");
            AnimationClip nowclip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (nowclip != null)
            {
                index = index + 50;
                point.x = 300;
                point.y = index;
                AddClipToAnimator(anim, nowclip, point);
            }
        }

    }

    #region 添加动画片段参数

    public static Dictionary<string, AnimatorControllerParameterType> ParameterDataDic=new Dictionary<string, AnimatorControllerParameterType> ();
    /// <summary>
    /// 初始化动画片段参数
    /// </summary>
    public static void InitParameterData()
    {
        ParameterDataDic.Clear();
        ParameterDataDic["YC_Int"] = AnimatorControllerParameterType.Int;
        ParameterDataDic["YC_Bool"] = AnimatorControllerParameterType.Bool;
        ParameterDataDic["YC_Floot"] = AnimatorControllerParameterType.Float;
        ParameterDataDic["YC_Trigger"] = AnimatorControllerParameterType.Trigger;
    }
    /// <summary>
    /// 添加动画片段参数
    /// </summary>
    public static void AddParameter(AnimatorController ac)
    {
        foreach (var item in ParameterDataDic)
        {
            ac.AddParameter(item.Key,item.Value);
        }
       
    }
    public static void ClearAllParameter(AnimatorController ac)
    {
        if (ac==null)
        {
            return;
        }
       
    }
    #endregion


    //添加一个Clip到动画机
    public static bool AddClipToAnimator(AnimatorController ac, AnimationClip item, Vector3 point)
    {
        if (!ac.animationClips.Contains(item))
        {
            AnimatorStateMachine sm = ac.layers[0].stateMachine;
          
            AnimatorState state = sm.AddState(item.name, point);
            state.motion = item;
            var transition = sm.AddAnyStateTransition(state);
            transition.hasExitTime = false;
            transition.duration = 0.2f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.Equals, 0, "LYCtext");
            EditorUtility.SetDirty(ac);
            return true;
        }
        else
        {
            return false;
        }
    }

}
