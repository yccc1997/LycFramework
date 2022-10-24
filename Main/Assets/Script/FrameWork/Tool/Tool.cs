using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
/// <summary>
/// 工具
/// </summary>
public class Tool
{
    #region IO操作

    /// <summary>
    /// 创建文件夹
    /// </summary>
    /// <param name="dir">文件夹路径</param>
    private static void CreatDirectory(string dir)
    {
        if (!Directory.Exists(dir))//如果不存在就创建 dir 文件夹  
            Directory.CreateDirectory(dir);

    }

    /// <summary>
    /// 写入数据（覆盖写入）
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="data">准备写入的数据</param>
    public static void WriterData(string path, string data)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(data);
            sw.Dispose();
            sw.Close();
        }
    }

    /// <summary>
    /// 得到文件MD5值
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public static string GetMD5HashFromFile(string path)
    {
        StringBuilder strBuilder = new StringBuilder();
        try
        {
            FileStream file = new FileStream(path, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();
            strBuilder.Length = 0;
            for (int i = 0; i < retVal.Length; i++)
            {
                strBuilder.Append(retVal[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 下载资源到本地
    /// </summary>
    /// <param name="url">资源路径</param>
    /// <param name="localPath">本地路径</param>
    public static void DownloadData(string url, string localPath, Action<object> callBack)
    {
        try
        {
            byte[] buffer = new byte[2048];
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            req.Method = "GET";
            req.Timeout = 120000;
            req.ReadWriteTimeout = 120000;
            req.Proxy = null;
            HttpWebResponse rsp = req.GetResponse() as HttpWebResponse;

            //string dirPath = localPath.Remove(localPath.LastIndexOf('/'));
            ////本地没有文件夹 创建文件夹
            //if (!Directory.Exists(dirPath)) { Directory.CreateDirectory(dirPath); }
            //本地有指定文件 删除
            if (File.Exists(localPath)) { File.Delete(localPath); }

            string tempLocalPath = localPath + "_temp";
            using (FileStream fileStream = new FileStream(tempLocalPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                using (Stream stream = rsp.GetResponseStream())
                {
                    int len = stream.Read(buffer, 0, buffer.Length);
                    while (len > 0)
                    {
                        fileStream.Write(buffer, 0, len);
                        fileStream.Flush();
                        len = stream.Read(buffer, 0, buffer.Length);
                    }
                }
                fileStream.Close();
                File.Move(tempLocalPath, localPath);
            }
            UnityEngine.Debug.Log("下载完成：" + localPath);
            callBack(localPath);
        }
        catch (System.Exception data)
        {

        }
    }


    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="path"></param>
    public static void WriterData(byte[] bytes, string path)
    {
        if (bytes == null)
        {
            return;
        }
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();       
        }
    }


    #endregion

    #region Math
    /// <summary>
    /// 将Long类型数值按照高32位和低32位拆分成两个数值
    /// </summary>
    /// <param name="data"></param>
    /// <param name="hight32"></param>
    /// <param name="low32"></param>
    public static void ConvertLongToTwoInt(long data, out int hight32, out int low32)
    {
        hight32 = (int)(data >> 32);
        low32 = (int)(data & 0xffffffff);
    }

    /// <summary>
    /// 将两个Int值按照高32位和低32位的方式生成一个long类型的数据
    /// </summary>
    /// <param name="high32"></param>
    /// <param name="low32"></param>
    /// <returns></returns>
    public static long ConvertTwoIntToLong(int high32, int low32)
    {
        return ((long)high32) << 32 | (low32 & 0xffffffff);
    }

    #endregion

    #region 文本操作
    /// <summary>
    /// 移除目标数据的色值
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string RemoveColorCode(string content)
    {
        return new System.Text.RegularExpressions.Regex("\\[[0-9A-Fa-f]{6}\\]|\\[-\\]", RegexOptions.IgnoreCase).Replace(content, "");
    }

    #endregion

    #region 单位转换值转换文本
    /// <summary>
    /// 数据值转换文本
    /// </summary>
    /// <param name="size_L"></param>
    /// <returns></returns>
    public static string DataUnitChangeStr(long size_L)
    {
        float size = (float)size_L;
        bool isNext = true;
        int index = 0;
        while (isNext)
        {
            if (size / 1024L >= 1)
            {
                size = size / 1024f;
                index++;
            }
            else
            {
                isNext = false;
            }
        }
        string monad = "";
        switch (index)
        {
            case 0:
                monad = "b";
                break;
            case 1:
                monad = "KB";
                break;
            case 2:
                monad = "MB";
                break;
            case 3:
                monad = "G";
                break;
            case 4:
                monad = "T";
                break;
        }
        return monad;
    }

    #endregion

    #region 随机数
    /// <summary>
    /// 返回一个min至max之间的整型随机数
    /// 内部调用了UnityEngine.Random.Range(int,int)
    /// </summary>
    /// <param name="min">最小值(包括在内)</param>
    /// <param name="max">最大值(不包括在内)</param>
    /// <returns></returns>
    public static int UnityEngineRandom_Int32(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    /// 返回一个min至max之间的单精度浮点型随机数
    /// 内部调用了UnityEngine.Random.Range(float,float)
    /// </summary>
    /// <param name="min">最小值(包括在内)</param>
    /// <param name="max">最大值(包括在内)</param>
    /// <returns></returns>
    public static float UnityEngineRandom_Float(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    #endregion

    #region 阿拉伯数字转中文大写到亿（有需要再加）

    private static string[] Word = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
    private static string[] Uint = new string[] { "个", "十", "百", "千", "万", "十", "百", "千", "亿" };
    private static List<string> wordList = new List<string>();
    private static StringBuilder SB = new StringBuilder();
    /// <summary>
    /// 阿拉伯数字转中文大写
    /// </summary>
    public static string ArabicNumeralsToWordNumbers(int num)
    {
        int uintCount = 1;
        wordList.Clear();
        while (num / 10 != 0)
        {
            int word = num % 10;
            wordList.Add(Word[word]);
            if (uintCount >= 1)
            {
                wordList.Add(Uint[uintCount]);
            }
            uintCount++;
            num = num / 10;
        }
        wordList.Add(Word[num]);
        wordList.Reverse();
        SB.Clear();
        for (int i = 0; i < wordList.Count; i++)
        {
            SB.Append(wordList[i]);
        }
        string final = SB.ToString();
        final = final.Replace("一十", "十");
        final = final.Replace("十零", "十");
        final = final.Replace("零零", "零");
        return final;
    }
    #endregion

    #region 颜色转换
    /// <summary>
    /// hex转颜色
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static Color HexToColor(string hex)
    {
        byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //byte cc = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        float r = br / 255f;
        float g = bg / 255f;
        float b = bb / 255f;
        float a = 1;
        return new Color(r, g, b, a);
    }

    /// <summary>
    /// color 转Hex
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static string ColorToHex(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255.0f);
        int g = Mathf.RoundToInt(color.g * 255.0f);
        int b = Mathf.RoundToInt(color.b * 255.0f);
        int a = Mathf.RoundToInt(color.a * 255.0f);
        string hex = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", r, g, b, a);
        return "[" + hex + "]";
    }
    #endregion

    #region 时间转换

    private static DateTime DefaultTime = new System.DateTime(1970, 1, 1, 8, 0, 0);
    /// <summary>
    /// 转成年月日时分秒
    /// </summary>
    /// <returns></returns>
    public string ParseTime(long timeStamp)
    {
        return DefaultTime.AddSeconds(timeStamp / 1000).ToString();
    }

    /// <summary>
    /// 输入字符串返回年月日数组
    /// </summary>
    /// <returns></returns>
    public static int[] GetYearMonthDayDescribeByMillisecond(long millisecond)
    {
        DateTime dataTime = DefaultTime.AddSeconds(millisecond / 1000);
        int[] dataTimeArray = new int[] { dataTime.Year, dataTime.Month, dataTime.Day };
        return dataTimeArray;
    }

    /// <summary>
    /// 获取时间（天）
    /// </summary>
    /// <returns></returns>
    public static int GetDayByMilliscond(long millisecond)
    {
        int day = DefaultTime.AddSeconds(millisecond / 1000).Day;
        return day;
    }

    /// <summary>
    /// 输入时间返回年月日字符串
    /// </summary>
    /// <returns></returns>
    public static string GetYearMonthDayDescribeTextByMillisecond(long millisecond)
    {
        int[] intArray = GetYearMonthDayDescribeByMillisecond(millisecond);
        string timeText = "{0}年{1}月{2}日";
        return string.Format(timeText, intArray);
    }
    #endregion

    #region AES加密解密
    /// <summary>
    /// 获取密钥
    /// </summary>
    private static string DefaultKey
    {
        get
        {
            return "pdqcexegx1hvc2q0";    ////必须是16位
        }
    }

    //默认密钥向量
    private static byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

    /// <summary>
    /// AES加密算法
    /// </summary>
    /// <param name="plainText">明文字符串</param>
    /// <returns>将加密后的密文转换为Base64编码，以便显示</returns>
    public static string AESEncrypt(string plainText, string key = null)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
            {
                key = DefaultKey;
            }
            //分组加密算法
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的字节数组
                                                                      //设置密钥及密钥向量
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = _key1;
            byte[] cipherBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();//得到加密后的字节数组
                    cs.Close();
                    ms.Close();
                }
            }
            cipherBytes[0] += 1;
            return Convert.ToBase64String(cipherBytes);
        }
        catch
        {
            UnityEngine.Debug.LogError("ARS加密失败");
            return "";
        }

    }

    /// <summary>
    /// AES解密
    /// </summary>
    /// <param name="cipherText">密文字符串</param>
    /// <returns>返回解密后的明文字符串</returns>
    public static string AESDecrypt(string showText, string key = null)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
            {
                key = DefaultKey;
            }

            byte[] cipherText = Convert.FromBase64String(showText);
            cipherText[0] -= 1;
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(key);
            des.IV = _key1;
            byte[] decryptBytes = new byte[cipherText.Length];
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(decryptBytes, 0, decryptBytes.Length);
                    cs.Close();
                    ms.Close();
                }
            }
            return Encoding.UTF8.GetString(decryptBytes).Replace("\0", "");   ///将字符串后尾的'\0'去掉
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("ARS解密失败:" + e);
            return "";
        }
    }

    public static string GeneraKey(string origion, string id)
    {
        string AESKey;
        string origionLen = origion.Length.ToString();
        if (origion.Length >= 16)
        {
            if (id.Length > 6)
            {
                int num1, num2, num3, num4;
                int.TryParse(id[5].ToString(), out num1);
                int.TryParse(id[7].ToString(), out num2);
                int.TryParse(id[10].ToString(), out num3);
                int.TryParse(id[13].ToString(), out num4);
                string a01 = id[num1].ToString();
                string a02 = id[num2].ToString();
                string a03 = id[num3].ToString();
                string a04 = id[num4].ToString();
                string a1 = origion.Substring(origion.Length - 16, (10 - origionLen.Length));
                string a2 = id.Substring(id.Length - 2, 1);
                string a3 = origionLen.Substring(0, origionLen.Length);
                string a4 = id.Substring(1, 1);
                a01 = AESEncrypt(a01, "dsfqwwe1oxciqwek")[6].ToString();
                a02 = AESEncrypt(a02, "asdzxcrqv412.qwe")[1].ToString();
                a03 = AESEncrypt(a03, "nmxlkcnzkhbkbarn")[3].ToString();
                a04 = AESEncrypt(a04, "gefkgjnskiwrwesa")[4].ToString();
                a2 = AESEncrypt(a2, "qdbcexagxx9ac178")[5].ToString();
                a4 = AESEncrypt(a4, "5bqcfxaaxbxvvbh6")[3].ToString();

                a1 = AESEncrypt(a1, "zlxicu81jg" + a02 + a03 + a01 + a2 + a04 + a4).Substring(0, (10 - origionLen.Length));
                AESKey = a01 + a1 + a02 + a2 + a03 + a3 + a04 + a4;
                //AESKey = origion.Substring(origion.Length - 16, (14 - origionLen.Length)) + id.Substring(id.Length - 2, 1)
                //    + origionLen.Substring(0, origionLen.Length) + id.Substring(0, 1);
            }
            else
            {
                AESKey = origion.Substring(origion.Length - 16, 16);
            }
        }
        else
        {
            AESKey = origion;
            for (int i = origion.Length; i < 16; i++)
            {
                AESKey += "0";
            }
        }
        return AESKey;
    }
    #endregion

    #region AES网络加密
    private static string mKey = "";
    private static string Key
    {
        get
        {

#if deleteRef_from_TableManager_cs_in_SundryTableManager_begin
if (string.IsNullOrEmpty (mKey) && SundryTableManager.Instance.dic.ContainsKey (50002u)) {
	TABLE.SUNDRY table = SundryTableManager.Instance [50002u];
	if (table != null)
		mKey = table.effect_name;
}
#endif
            //delete_Ref_end

            return mKey;
        }
    }

    private static byte[] mKeyArray = null;
    private static byte[] KeyArray
    {
        get
        {
            if (mKeyArray == null && !string.IsNullOrEmpty(Key))
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                mKeyArray = md5.ComputeHash(Encoding.UTF8.GetBytes(Key));
            }
            return mKeyArray;
        }
    }

    private static RijndaelManaged mRijn = null;
    private static RijndaelManaged Rijn
    {
        get
        {
            if (mRijn == null)
            {
                mRijn = new RijndaelManaged();
                mRijn.Key = KeyArray;
                mRijn.Mode = CipherMode.ECB;
                mRijn.Padding = PaddingMode.PKCS7;
            }
            return mRijn;
        }
    }

    /// <summary>    
    /// AES加密    
    /// </summary>    
    /// <param name="encryptStr">明文</param>    
    /// <returns></returns>    
    public static byte[] AesNetEncrypt(byte[] encryptArray)
    {
        if (KeyArray == null)
        {
            return encryptArray;
        }

        return Rijn.CreateEncryptor().TransformFinalBlock(encryptArray, 0, encryptArray.Length);
    }

    /// <summary>    
    /// AES解密    
    /// </summary>    
    /// <param name="decryptArray">密文</param>    
    /// <returns></returns>    
    public static byte[] AesNetDecrypt(byte[] decryptArray)
    {
        if (KeyArray == null)
        {
            return decryptArray;
        }

        return Rijn.CreateDecryptor().TransformFinalBlock(decryptArray, 0, decryptArray.Length);
    }
    #endregion
}

public enum EAesOperatingType
{
    /// <summary>
    /// 加密
    /// </summary>
    Encryption,
    /// <summary>
    /// 解密
    /// </summary>
    Decrypt,

}
