using Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Network : MonoBehaviour
{
    private ClientSocket mSocket;
    public string SendMessageDes = "";
    void Start()
    {
        mSocket = new ClientSocket();
        mSocket.ConnectServer("192.168.137.1", 8088);
        mSocket.SendMessage("啦啦啦！");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            mSocket.SendMessage(SendMessageDes);
        }
    }


}
