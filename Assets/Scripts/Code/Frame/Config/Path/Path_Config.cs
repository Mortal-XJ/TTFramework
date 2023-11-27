using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Config 
{
    public static string RootPath
    {
        get
        {
            string rootPath = "";
            // 根据不同平台设置不同的路径
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    rootPath = Application.persistentDataPath;
                    break;
                case RuntimePlatform.IPhonePlayer:
                    rootPath = Application.persistentDataPath;
                    break;
                case RuntimePlatform.WebGLPlayer:
                    // 在Web平台，可以使用Application.dataPath或Application.persistentDataPath，具体取决于您的需求
                    rootPath = Application.persistentDataPath;
                    break;
                default:
                    // 默认情况下，使用Application.dataPath
                    rootPath = Application.dataPath;
                    break;
            }

            return rootPath;
        }
    }
}
