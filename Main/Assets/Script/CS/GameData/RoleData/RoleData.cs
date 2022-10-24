using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleData
{
    private string name;
    private int id;
    private string modelPath;
    private Vector3 nowCell;
    private Vector3 nextCell;
    private ERoleState state;
    private EDirection direction;
    private Vector3 direction_v3;
    private Vector3 changePos_v3;
    private float speed = 0.05f;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get => name; set => name = value; }
    /// <summary>
    /// id
    /// </summary>
    public int Id { get => id; set => id = value; }
    /// <summary>
    /// 当前位置
    /// </summary>
    public Vector3 NowCell { get => nowCell; set => nowCell = value; }
    /// <summary>
    /// 目标点位置
    /// </summary>
    public Vector3 NextCell { get => nextCell; set => nextCell = value; }
    /// <summary>
    /// 状态
    /// </summary>
    public ERoleState State { get => state; set => state = value; }
    /// <summary>
    /// 方向
    /// </summary>
    public EDirection Direction { get => direction; set => direction = value; }
    public string ModelPath { get => modelPath; set => modelPath = value; }
    public Vector3 Direction_v3 { get => direction_v3; set => direction_v3 = value; }

    public RoleData(int id, string modelPath)
    {
        this.Id = id;
        this.ModelPath = modelPath;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
    public void Move(EDirection dir)
    {
        if (changePos_v3==null)
        {
            changePos_v3 = new Vector3();
        }
        if (Direction_v3==null)
        {
            Direction_v3 = new Vector3();
        }
        changePos_v3 = Vector3.zero;
        switch (dir)
        {
            case EDirection.Forward:
                changePos_v3.z += speed;
                direction_v3.y = 0;
                break;
            case EDirection.Backward:
                changePos_v3.z -= speed;
                direction_v3.y = 180;
                break;
            case EDirection.Left:
                changePos_v3.x -= speed;
                direction_v3.y = 270;
                break;
            case EDirection.Right:
                changePos_v3.x += speed;
                direction_v3.y = 90;
                break;
            default:
                break;
        }
        NowCell += changePos_v3;
    }
}
