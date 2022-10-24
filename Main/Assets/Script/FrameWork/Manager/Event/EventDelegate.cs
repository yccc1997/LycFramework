using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDelegate
{
    public delegate void Callback(int id, params object[] data);
    public List<Callback> arrCallBack = new List<Callback>();
    public List<Callback> arr2CallBack = new List<Callback>();



}
