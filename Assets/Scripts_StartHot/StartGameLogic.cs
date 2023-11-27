using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using UnityEngine.AddressableAssets;



public class StartGameLogic : MonoBehaviour
{
    void Awake()
    {
        GetRemoteAssembly().Forget();
    }
    
    #region HotUpdate

    private static Assembly _hotUpdateAss;
    //存储热更程序字节
    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    //获取程序字节
    public static byte[] ReadBytesFromStreamingAssets(string dllName)
    {
        return s_assetDatas[dllName];
    }

    //AOT文件程序集
    private static List<string> AOTMetaAssemblyFiles { get; } = new List<string>()
    {
       "UniTask.dll",
       "System.dll",
       "mscorlib.dll"
    };
    async UniTask GetRemoteAssembly()
    {
       //热更程序集
        var  HotUpdates= new List<string>
        {
            "HotUpdate.dll",
        }.Concat(AOTMetaAssemblyFiles);

        foreach (var asset in HotUpdates)
        {   
            string assemblyKey = asset;
            TextAsset textAsset =
                await Addressables.LoadAssetAsync<TextAsset>(assemblyKey).ToUniTask();
            s_assetDatas.Add(assemblyKey, textAsset.bytes);
        }
        StartHot(); //开始启动热更程序集
    }

    void StartHot()
    {
        LoadMetadataForAOTAssemblies();
#if !UNITY_EDITOR
        _hotUpdateAss = Assembly.Load(ReadBytesFromStreamingAssets("HotUpdate.dll"));
#else
        _hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#endif
        Type entryType = _hotUpdateAss.GetType("HotMain");
        entryType.GetMethod("Start").Invoke(null, null);
    }

    // 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    // 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    private static void LoadMetadataForAOTAssemblies()
    {
        // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyFiles)
        {
            byte[] dllBytes = ReadBytesFromStreamingAssets(aotDllName);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }
    #endregion
}