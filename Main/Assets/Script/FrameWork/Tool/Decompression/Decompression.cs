using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System;

/// <summary>
/// 解压
/// </summary>
public class Decompression
{
    private static Decompression instance;

    public static Decompression Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Decompression();
                instance.StartDecompression();
            }
            return instance;
        }
    }
    /// <summary>
    ///等待解压列表
    /// </summary>
    private List<UnZip> waitDecompressionList = new List<UnZip>();

    private Thread zipProcessThread;

    private static readonly object objLock = new object();
    private void StartDecompression()
    {
        OpenThread();
    }

    /// <summary>
    /// 添加解压任务
    /// </summary>
    public void AddUnZipTask(string startPath, string endPath, Action<object> callback)
    {
        if (waitDecompressionList==null)
        {
            waitDecompressionList = new List<UnZip>();
        }
        UnZip SpecialRes = new UnZip(startPath, endPath, callback);
        waitDecompressionList.Add(SpecialRes);
    }

    private void OpenThread()
    {
        zipProcessThread = new Thread(ZipAsync);
        zipProcessThread.Start();
    }


    private void ZipAsync()
    {
        if (waitDecompressionList == null)
        {
            return;
        }
        while (true)
        {
            if (waitDecompressionList.Count > 0)
            {
                try
                {
                    UnZip zip;
                    lock (objLock)
                    {
                        zip = waitDecompressionList[0];
                        waitDecompressionList.RemoveAt(0);
                    }
                    zip.Decompression();
                    UnityEngine.Debug.Log("解压完成：" + zip.ZipFile);

                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.Log("ex：" + ex);
                }
            }
           
            Thread.Sleep(10);
        }

    }
}

