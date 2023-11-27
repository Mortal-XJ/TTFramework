using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameFrame
{
    [System.Serializable]
    public class UIFrameWindData 
    {
        /// <summary>
        /// 脚本保存路径
        /// </summary>
        public string _scriptSavePath;

        public string ScriptSavePath
        {
            get
            {
                if (_scriptSavePath == null || _scriptSavePath == "")
                {
                    _scriptSavePath = System.IO.Path.Combine(Application.dataPath, "Scripts/Logic/UIMono");
                }

                return _scriptSavePath;
            }
            set { _scriptSavePath = value; }
        }

        /// <summary>
        /// 生成脚本头校验字符
        /// </summary>
        public string UIDlgKey = "T";

        /// <summary>
        /// 生成可生成的组件脚本的头校验字符
        /// </summary>
        public string UIControlKey = "TC";

        /// <summary>
        /// UI预制体加载模式
        /// </summary>
        public ResourceType UILoadMode = ResourceType.ADDRESSABLES;

        /// <summary>
        /// UI预制体存放的文件夹路径
        /// </summary>
        public string UIPrefabAssetPath = "UIPrefabAsset/Dlg/";

        /// <summary>
        /// UI脚本默认层级
        /// </summary>
        public UITier UIScriptDefaultTier = 0;

        /// <summary>
        /// 如果创建脚本时 这个脚本已经存在是否进行覆盖
        /// </summary>
        public bool ICSEWTOverwrite = true;
    }

    public class UIFrameWindDataUtility
    {
        private static string loadPathEditor = Application.dataPath + "/HotAssets/UI/UIData/UIFrameWindData_Et.json";
        private static string AAName = "UIFrameWindData_Et";

        /// <summary>
        /// 获取UIFrameWindData实例。如果不存在，则创建并返回新实例。
        /// </summary>
        /// <returns>UIFrameWindData实例。</returns>
        public static async UniTask<UIFrameWindData> GetUIFrameWindDatsAsync()
        {
            UIFrameWindData data =default;
#if !UNITY_EDITOR
            UnityEngine.TextAsset jsonHandle = await Addressables.LoadAssetAsync<UnityEngine.TextAsset>(AAName).ToUniTask();
            string json = jsonHandle.text;
                data = JsonUtility.FromJson<UIFrameWindData>(json);
            if (data == null)
                data = new UIFrameWindData();
            return data;

#endif
            return data;

        } 
#if UNITY_EDITOR    
        public static UIFrameWindData GetUIFrameWindData_Editor()
        {
            UIFrameWindData data;

            if (!System.IO.File.Exists(loadPathEditor))
            {
                data = new UIFrameWindData();
                FolderValidation();
                string json = JsonUtility.ToJson(data);
                System.IO.File.WriteAllText(loadPathEditor, json);
            }
            else
            {
                string json = System.IO.File.ReadAllText(loadPathEditor);
                data = JsonUtility.FromJson<UIFrameWindData>(json);
            }

            return data;

        }
        /// <summary>
        /// 设置UIFrameWindData实例。
        /// </summary>
        /// <param name="data">要设置的UIFrameWindData实例。</param>
        public static void SetUIFrameWindData(UIFrameWindData data)
        {
            if (!System.IO.Directory.Exists(loadPathEditor))
                FolderValidation();
            string json = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText(loadPathEditor, json);
            
        }

        static void FolderValidation()
        {
            // 获取文件所在文件夹路径
            string folderPath =System.IO.Path.GetDirectoryName(loadPathEditor);

            // 检查文件夹是否存在，如果不存在则创建
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);
        }
        
#endif
    }
}