using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundlePackage : MonoBehaviour
{
    [MenuItem("Tool/BuildAssetbundle")]
    static void BuildABs()
    {
        //打包后的资源存放路径，这个路径根据项目情况不定
        string abDataPath = @"D:\Data\Project\LYC\AssetBundlePackage";

        print(Application.dataPath);
        string shaderPath = Application.dataPath+ "//AssetBundleRes//shader";
        string texturePath = Application.dataPath + "//AssetBundleRes//texture";
        string materialPath = Application.dataPath + "//AssetBundleRes//material";
        string prefabPath = Application.dataPath + "//AssetBundleRes//prefab";
        string other = Application.dataPath + "//AssetBundleRes//other";
        List<AssetBundleBuild> abList = new List<AssetBundleBuild>();
        AddAssetBundleBuild(shaderPath, ref abList);
        AddAssetBundleBuild(texturePath, ref abList);
        AddAssetBundleBuild(materialPath, ref abList);
        AddAssetBundleBuild(prefabPath, ref abList);
        AddAssetBundleBuild(other, ref abList);

        BuildPipeline.BuildAssetBundles(abDataPath, abList.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        Debug.Log("AssetBundle打包完毕:" + abDataPath);
        return;
        
        if (!Directory.Exists(abDataPath)) Directory.CreateDirectory(abDataPath);


        //BuildAssetBundle函数后的三个参数分别为：存储路径；压缩方式；资源目标平台
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        AssetBundleBuild ab1 = new AssetBundleBuild();
        ab1.assetBundleName = "cs/t1";
        ab1.assetNames = new string[1] { "Assets/Resource/0000.png" };

        AssetBundleBuild ab2 = new AssetBundleBuild();
        ab2.assetBundleName = "c1";
        ab2.assetNames = new string[1] { "Assets/Resource/c1.prefab" };

        AssetBundleBuild ab3 = new AssetBundleBuild();
        ab3.assetBundleName = "mat1";
        ab3.assetNames = new string[1] { "Assets/Resource/mat1.mat" };

        AssetBundleBuild ab4 = new AssetBundleBuild();
        ab4.assetBundleName = "cs/t2";
        ab4.assetNames = new string[1] { "Assets/Resource/1.jpg" };


        AssetBundleBuild ab5 = new AssetBundleBuild();
        ab5.assetBundleName = "s1";
        ab5.assetNames = new string[1] { "Assets/Resource/s1.shader" };

        AssetBundleBuild ab7 = new AssetBundleBuild();
        ab7.assetBundleName = "c2";
        ab7.assetNames = new string[1] { "Assets/Resource/c2.prefab" };



        list.Add(ab1);
        list.Add(ab2);
        list.Add(ab3);
        list.Add(ab4);
        list.Add(ab5);
        list.Add(ab7);
        BuildPipeline.BuildAssetBundles(abDataPath, list.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        Debug.Log("AssetBundle打包完毕:"+ abDataPath);
    }

    public static void AddAssetBundleBuild(string path,ref List<AssetBundleBuild> list)
    {
        if (Directory.Exists(path))
        {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles("*", SearchOption.AllDirectories);
            int maxLength = files.Length;
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".meta")) continue;
                string name = files[i].Name.Substring(0, files[i].Name.LastIndexOf('.')); 
                string abpath = files[i].DirectoryName.Replace("\\","/").Replace(Application.dataPath+"/", "");
                string abName = abpath + "/" + name;
                string dataName = files[i].DirectoryName.Replace("\\", "/").Replace(Application.dataPath, "Assets") + "/" + files[i].Name;
                AssetBundleBuild ab = new AssetBundleBuild();
                ab.assetBundleName = abName;
                ab.assetNames = new string[1] { dataName };
                list.Add(ab);
            }
        }
    }





}
