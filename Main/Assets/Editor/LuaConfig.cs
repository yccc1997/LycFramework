using System;
using UnityEngine;
using XLua;

public static class LuaConfig
{
    [CSharpCallLua]
    public static Type[] CSharpCallLua = new Type[]
    {
        typeof(Action<string>),
        typeof(Application.LogCallback),
        typeof(Action<float, float>),
    
    };

 //   [LuaCallCSharp]
 //   public static Type[] LuaCallCSharp = new Type[]
 //   {
 //       typeof(DG.Tweening.DOTweenAnimationExtensions),
 //       typeof(DG.Tweening.ShortcutExtensions),
 //       typeof(DG.Tweening.ShortcutExtensions43),
 //       typeof(DG.Tweening.ShortcutExtensions46),
 //       typeof(DG.Tweening.ShortcutExtensions50),
 //       typeof(DG.Tweening.ShortcutExtensionsPro),
 //       typeof(DG.Tweening.TweenExtensions),
 //       typeof(DG.Tweening.TweenSettingsExtensions),

 //       //DoTween
	//	typeof(DG.Tweening.UpdateType),
	//	typeof(DG.Tweening.TweenType),
	//	typeof(DG.Tweening.ScrambleMode),
	//	typeof(DG.Tweening.RotateMode),
	//	typeof(DG.Tweening.PathType),
	//	typeof(DG.Tweening.PathMode),
	//	typeof(DG.Tweening.LoopType),
	//	typeof(DG.Tweening.LogBehaviour),
	//	typeof(DG.Tweening.Ease),
	//	typeof(DG.Tweening.AxisConstraint),
	//	typeof(DG.Tweening.AutoPlay),


	//	//typeof(DG.Tweening.DOTween),
	//	typeof(DG.Tweening.DOVirtual),
	//	typeof(DG.Tweening.EaseFactory),
	//	typeof(DG.Tweening.Tweener),
	//	typeof(DG.Tweening.Tween),
	//	typeof(DG.Tweening.Sequence),
	//	typeof(DG.Tweening.TweenParams),
	//	typeof(DG.Tweening.Core.ABSSequentiable),

	//	typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),
	//	typeof(DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>),

	//	typeof(DG.Tweening.TweenCallback),
	//	typeof(DG.Tweening.TweenExtensions),
	//	typeof(DG.Tweening.TweenSettingsExtensions),
	//	typeof(DG.Tweening.ShortcutExtensions),
	//	typeof(DG.Tweening.ShortcutExtensions43),
	//	typeof(DG.Tweening.ShortcutExtensions46),
	//	typeof(DG.Tweening.ShortcutExtensions50),
   
	//	//dotween pro 的功能
	//	typeof(DG.Tweening.DOTweenPath),
	//	typeof(DG.Tweening.DOTweenVisualManager),


	//	typeof(UnityEngine.Matrix4x4),
	//	typeof(UnityEngine.ParticleSystem.MinMaxGradient),
	//	typeof(UnityEngine.Rect),
	//	typeof(Framework.HUDTitleInfo.HUDTitleBatcher),
	//	typeof(UnityEngine.EventSystems.EventSystem),
	//	typeof(UnityEngine.LayerMask),
	//	typeof(Framework.RoleAnimatorSpeed),
	//	typeof(UnityEngine.AssetBundle),
	//	typeof(UnityEngine.CustomYieldInstruction),

	//	typeof(MobileInfo),
	//	typeof(Framework.HUDTilteLine),
	//	typeof(Framework.HUDTitleInfo.HUDTitleRender),
	//};
}
