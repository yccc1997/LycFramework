using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色状态
/// </summary>
public enum ERoleState
{
    /// <summary>
    /// 无状态
    /// </summary>
    None,
    /// <summary>
    /// 待机
    /// </summary>
    Stand,
    /// <summary>
    /// 奔跑
    /// </summary>
    Run,
    /// <summary>
    /// 攻击
    /// </summary>
    Attack,
    /// <summary>
    /// 死亡
    /// </summary>
    Dead,
}
/// <summary>
/// 方向
/// </summary>
public enum EDirection
{
    /// <summary>
    /// 前
    /// </summary>
    Forward,
    /// <summary>
    /// 后
    /// </summary>
    Backward,
    /// <summary>
    /// 左
    /// </summary>
    Left,
    /// <summary>
    /// 右
    /// </summary>
    Right,
    /// <summary>
    /// 前左
    /// </summary>
    Forward_Left,
    /// <summary>
    /// 前右
    /// </summary>
    Forward_Right,
    /// <summary>
    /// 后左
    /// </summary>
    Backward_Left,
    /// <summary>
    /// 后右
    /// </summary>
    Backward_Right,
}
