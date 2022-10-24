using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System;
public class UnZip
{
    private string zipFile;
    /// <summary>
    /// 需要解压的Zip文件（绝对路径）
    /// </summary>
    public string ZipFile { get { return zipFile; } }

    private string targetDirectory;
    /// <summary>
    /// 解压到的目录
    /// </summary>
    public string TargetDirectory { get { return targetDirectory; } }


    private int curSize;
    /// <summary>
    /// 当前解压进度
    /// </summary>
    public int CurSize { get { return curSize; } }

    private Action<object> callback;
    public UnZip(string ZipFile, string TargetDirectory, Action<object> callback)
    {
        this.zipFile = ZipFile;
        this.targetDirectory = TargetDirectory;
        this.callback = callback;
    }


    /// <summary>
    /// 解压zip格式的文件。
    /// </summary>
    public void Decompression()
    {
        try
        {
            string zipFilePath = ZipFile.Replace("\\", "/");
            string unZipDir = TargetDirectory.Replace("\\", "/");
            UnityEngine.Debug.Log("压缩文件路径:" + zipFilePath + "  解压到的路径:" + TargetDirectory);
            if (!File.Exists(zipFilePath))
            {
                UnityEngine.Debug.LogError("压缩文件不存在:" + zipFilePath);
                return;
            }
            if (!unZipDir.EndsWith("/"))
            {
                unZipDir += "/";
            }
            if (!Directory.Exists(unZipDir))
            {
                Directory.CreateDirectory(unZipDir);
            }
            byte[] data = new byte[4096];
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.Size == 0)
                    {
                        continue;
                    }
                    string name = theEntry.Name.Replace("\\", "/");
                    string directoryName = Path.GetDirectoryName(name).Replace("\\", "/");
                    string fileName = Path.GetFileName(name).Replace("\\", "/");
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(unZipDir + directoryName);
                    }
                    if (!directoryName.EndsWith("/"))
                        directoryName += "/";

                    string entryPath = unZipDir + name;
                    if (File.Exists(entryPath))
                    {
                        File.Delete(entryPath);
                    }
                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(unZipDir + name))
                        {
                            int size = 4096;
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                curSize += size;

                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            this.callback(true);
        }
        catch (System.Exception ex)
        {
           UnityEngine.Debug.Log("解压资源失败: " + ex);
        }
    }
}