//----------------------
// Developer Mortal
// Date 2023 - 10 - 23 
// Script Overview 
//----------------------

using System;
using System.IO;
using System.Text;
using GameFrame;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngineInternal;

namespace GameLogic
{
    public class UIFrameEditor : EditorWindow
    {
        private static UIFrameWindData UIFrameData => UIFrameWindDataUtility.GetUIFrameWindData_Editor();

        private static string _saveRootPath => UIFrameData.ScriptSavePath;

        static StringBuilder _componentData = new StringBuilder();
        static StringBuilder _ExceptForBinding = new StringBuilder();

        //选中的物体
        private static GameObject root;

        //创建UI代码
        [MenuItem("GameObject/CreatorUIScript", false, 0)]
        static void CreatorUIScript()
        {
            try
            {
                string uiName = Selection.activeObject.name;
                int length = UIFrameData.UIDlgKey.Length > uiName.Length ? uiName.Length : UIFrameData.UIDlgKey.Length;
                uiName = uiName == null ? "null" : uiName.Substring(0, length);

                if (uiName == UIFrameData.UIDlgKey)
                {
                    root = Selection.activeObject as GameObject;
                    GetGenerateUIScriptInfo(root);

                    //创建控件控件
                    string templatePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
                        "GameFrameConfig/UI/ControlTemplate.txt");
                    string template = File.ReadAllText(templatePath);
                    string className = $"{root.name}_Component";
                    template = template.Replace("#ComponentControl#", _componentData.ToString());
                    template = template.Replace("#ExceptForBinding#", _ExceptForBinding.ToString());
                    template = template.Replace("#ClassName#", className);
                    CreateClass($"{root.name}/UILogicBase", className, template);
                    _componentData.Clear();
                    _ExceptForBinding.Clear();

                    //创建 Model
                    templatePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
                        "GameFrameConfig/UI/UI_WindowMonoModel.txt");
                    template = File.ReadAllText(templatePath);
                    string className_model = $"{root.name}_Model";
                    template = template.Replace("#ClassName#", className_model);
                    CreateClass($"{root.name}", className_model, template, false);
                    //创建ControlBase
                    templatePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
                        "GameFrameConfig/UI/UI_WindowMonoBase.txt");
                    template = File.ReadAllText(templatePath);
                    string className_ControlBase = $"{root.name}_Mono";
                    template = template.Replace("#ClassName#", className_ControlBase);
                    template = template.Replace("#Control#", className);
                    template = template.Replace("#Model#", className_model);
                    template = template.Replace("#DefaultTier#", $"UITier.{UIFrameData.UIScriptDefaultTier}");
                    CreateClass($"{root.name}/UILogicBase", className_ControlBase, template);
                    //创建co
                    templatePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
                        "GameFrameConfig/UI/UI_WindowMono.txt");
                    template = File.ReadAllText(templatePath);
                    string className_Control = $"{root.name}";
                    template = template.Replace("#ClassName#", className_Control);
                    template = template.Replace("#ClassBaseName#", className_ControlBase);
                    CreateClass($"{root.name}", className_Control, template, false);

                    Log.Warning($"{root.name} UI脚本创建完成", Color.green);

                    AssetDatabase.Refresh();
                }
                else
                    Log.Warning("未发现有可生成的UI交互界面", Color.cyan);
            }
            catch (Exception e)
            {
                Log.Error("生成UI交互界面异常" + e);
            }
        }

        static void GetGenerateUIScriptInfo(GameObject goBase)
        {
            string key = UIFrameData.UIControlKey;
            Transform baseTans = goBase.transform;

            for (int i = 0; i < baseTans.childCount; i++)
            {
                Transform child = baseTans.GetChild(i);

                string uiName = child.name;
                int length = UIFrameData.UIControlKey.Length > uiName.Length
                    ? uiName.Length
                    : UIFrameData.UIControlKey.Length;

                uiName = uiName == null ? "null" : uiName.Substring(0, length);
                if (uiName == key)
                {
                    //获取信息
                    GetObjectComponents(child.gameObject, out string componentDataOut, out string exceptForBindingOut);
                    _componentData.Append(componentDataOut);
                    _ExceptForBinding.Append(exceptForBindingOut);
                }

                //继续遍历
                GetGenerateUIScriptInfo(child.gameObject);
            }
        }

        /// <summary>
        /// 生成一个对象上的组件信息
        /// </summary>
        /// <param name="gameObject"></param>
        static void GetObjectComponents(GameObject gameObject, out string componentData_out,
            out string exceptForBinding_out)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(gameObject.name, @"\s+$"))
            {
                Log.Error($"{gameObject.name} 后缀有空格请删除");
                exceptForBinding_out = "";
                componentData_out = "";
                return;
            }

            StringBuilder stringBuilder_componentData = new StringBuilder();
            StringBuilder stringBuilder_ExceptForBinding = new StringBuilder();

            //生成属性
            Component[] allComponents = gameObject.GetComponents<Component>();

            foreach (var component in allComponents)
            {
                System.Type tempType = component.GetType();
                string componentData = ComponentTemplateReplacement(gameObject, tempType, gameObject);
                stringBuilder_componentData.Append(componentData + "\n");
            }

            //生成取消绑定
            foreach (var component in allComponents)
            {
                System.Type tempType = component.GetType();
                //获取基本数据
                string componentClassName = tempType.Namespace + "." + tempType.Name;
                string componentControlName = gameObject.name + "_" + tempType.Name;

                string except = $"this._{componentControlName}  = null; \n            ";
                stringBuilder_ExceptForBinding.Append(except);
            }

            componentData_out = stringBuilder_componentData.ToString();
            exceptForBinding_out = stringBuilder_ExceptForBinding.ToString();
        }


        /// <summary>
        /// 生成组件代码数据
        /// </summary>
        /// <param name="component"></param>
        /// <param name="getTheGameObjectControl"></param>
        /// <returns></returns>
        static string ComponentTemplateReplacement(GameObject currentGo, Type component,
            GameObject getTheGameObjectControl)
        {
            //获取数据模板
            string templatePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
                "GameFrameConfig/UI/ControlAttributeTemplate.txt");
            string template = File.ReadAllText(templatePath);

            //获取基本数据
            string componentClassName = component.Namespace + "." + component.Name;
            string componentControlName = getTheGameObjectControl.name + "_" + component.Name;
            string transPath = GetRelativePath(currentGo, root);

            //替换数据
            template = template.Replace("#VariableControl#", componentClassName);
            template = template.Replace("#AttributeControl#", componentClassName);
            template = template.Replace("#VariableControlName#", componentControlName);
            template = template.Replace("#AttributeControlName#", componentControlName);
            template = template.Replace("#GetTheGameObjectControl#", transPath);
            return template;
        }

        // 获取从指定 GameObject 到基础 GameObject 的相对路径
        static string GetRelativePath(GameObject current, GameObject baseObject)
        {
            if (current == null)
                return "";

            if (current == baseObject)
                return "";

            string parentPath = GetRelativePath(current.transform.parent.gameObject, baseObject);

            if (!string.IsNullOrEmpty(parentPath))
            {
                return parentPath + "/" + current.name;
            }
            else
            {
                return current.name;
            }
        }

        static void CreateClass(string path, string fileName, string data, bool canForceABuild = true)
        {
            //创建文件夹
            string tempPath = Path.Combine(_saveRootPath, path);
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            //写入文件
            string writePath = Path.Combine(tempPath, fileName + ".cs");
            if (!File.Exists(writePath))
            {
                File.WriteAllText(writePath, data);
            }
            else if (UIFrameData.ICSEWTOverwrite && canForceABuild)
            {
                Log.Warning($"{writePath} AlreadyExistsButToForceBuild");
                File.Delete(writePath);
                File.WriteAllText(writePath, data);
            }
        }
    }

    public class UIFrameWindEditor : EditorWindow
    {
        private static UIFrameWindData _frameData;
        private static string ScriptSavePath;
        private static string UIDlgKey;
        private static string UIControlKey;
        private static string UIPrefabAssetPath;
        private static bool ICSEWTOverwrite;
        private static UITier UIScriptDefaultTier;
        private static ResourceType UILoadMode;

        private GUIStyle labelStyle;

        [MenuItem("GameFrame/UIWindHandel")]
        static void ShowWindow()
        {
            _frameData = UIFrameWindDataUtility.GetUIFrameWindData_Editor();
            ScriptSavePath = _frameData.ScriptSavePath;
            UIDlgKey = _frameData.UIDlgKey;
            UIControlKey = _frameData.UIControlKey;
            UILoadMode = _frameData.UILoadMode;
            UIPrefabAssetPath = _frameData.UIPrefabAssetPath;
            UIScriptDefaultTier = _frameData.UIScriptDefaultTier;
            ICSEWTOverwrite = _frameData.ICSEWTOverwrite;
            UIFrameWindEditor window = GetWindow<UIFrameWindEditor>("UIWindHandel");
            window.Show();
        }

        private void OnEnable()
        {
            try
            {
                labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.normal.textColor = Color.gray;
                labelStyle.alignment = TextAnchor.LowerLeft;
            }
            catch (Exception e)
            {
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            UIFrameConfigData();
            SetPrefabAssetPath();
            GUILayout.Space(10);
            DrawCreateUIRootNode();
            GUILayout.Space(20);
            GUILayout.Label(
                $"ScriptSavePath: 生成UI代码的保存路径\nUIDlgKey:选中生成UI脚本的GameObject,前缀必须等于这个Key\nUIControlKey:作用与‘nUIDlgKey’相同,只是作用域在UI下面的控件是否需要生成为代码。如果需要生成代码,就在控件前方添加这个Key\n额，我有时间出个教程",
                labelStyle);
        }

        /// <summary>
        /// ui框架基础配置信息
        /// </summary>
        private void UIFrameConfigData()
        {
            GUILayout.Label("脚本创建配置信息", labelStyle);
            ScriptSavePath = EditorGUILayout.TextField("ScriptSavePath", ScriptSavePath);
            UIDlgKey = EditorGUILayout.TextField("UIDlgKey", UIDlgKey);
            UIControlKey = EditorGUILayout.TextField("UIControlKey", UIControlKey);
            UILoadMode = (ResourceType)EditorGUILayout.EnumPopup("UILoadMode", UILoadMode);
            UIScriptDefaultTier = (UITier)EditorGUILayout.EnumPopup("UIDefaultShowTier", UIScriptDefaultTier);
            GUILayout.Space(5);

            GUILayout.Label("Tips: 如果要创建的UI脚本已经存在是否强制创建（会删除以前的脚本并创建新的）", labelStyle);
            ICSEWTOverwrite = EditorGUILayout.Toggle("ForceCreationScript", ICSEWTOverwrite);
            GUILayout.Label("创建完成后是否挂载脚本");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SaveData"))
            {
                SaveFrameData();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 设置预制体默认存放位置
        /// </summary>
        void SetPrefabAssetPath()
        {
            if (UILoadMode == ResourceType.RESOURCE)
            {
                GUILayout.Space(10);

                GUILayout.Label("Tips: UI预制体存放的文件夹", labelStyle);
                UIPrefabAssetPath = EditorGUILayout.TextField("UIPrefabAssetPath", UIPrefabAssetPath);
                if (GUILayout.Button("SaveDataAndCreateFolder"))
                {
                    if (UILoadMode == ResourceType.RESOURCE)
                    {
                        string resPath = Path.Combine(Application.dataPath, "Resources", UIPrefabAssetPath);
                        if (!Directory.Exists(resPath))
                            Directory.CreateDirectory(resPath);
                    }
                    else
                    {
                        string addPath = Path.Combine(Application.dataPath, UIPrefabAssetPath);
                        if (!Directory.Exists(addPath))
                            Directory.CreateDirectory(addPath);
                    }

                    SaveFrameData();
                    Log.Debug("CreateDefaultDirectory Finish");
                    AssetDatabase.Refresh();
                    GUIUtility.ExitGUI();
                }
            }
        }

        /// <summary>
        /// 根据UI层级枚举创建出Hierarchy根路径
        /// </summary>
        private void DrawCreateUIRootNode()
        {
            GUILayout.Label("Tips: 根据UI层级枚举创建出所需数据, 如:Hierarchy根路径", labelStyle);
            if (GUILayout.Button("CreateRootNode"))
            {
                StringBuilder scripts = new StringBuilder();
                StringBuilder scripts_Case = new StringBuilder();

                System.Array rootLevel = Enum.GetValues(typeof(UITier));
                for (int i = 0; i < rootLevel.Length; i++)
                {
                    UITier uiTier = (UITier)rootLevel.GetValue(i);
                    CreateUIRootNode(uiTier);
                    string template = CreateUIRootNodeScript(uiTier);
                    scripts.Append(template);
                    //创建Case判断
                    string tempCase = @"
                    case ""#VariableName#"":
                        trans = #VariableName#;
                    break;";
                    tempCase = tempCase.Replace("#VariableName#", $"{uiTier}_Root");
                    scripts_Case.Append(tempCase);
                }

                string templatePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
                    "GameFrameConfig/UI/UIRootNodeHandel.txt");
                string template_Class = File.ReadAllText(templatePath);
                template_Class = template_Class.Replace("#BreakIn#", scripts.ToString());
                template_Class = template_Class.Replace("#Case#", scripts_Case.ToString());

                CreateClass("UIRootNodeHandel", template_Class);

                scripts.Clear();
                scripts_Case.Clear();
                GameFrame.Log.Debug("UIRootNode 创建完成");
                GUIUtility.ExitGUI();
                AssetDatabase.Refresh();
            }
        }

        //创建一个UIRoot节点
        void CreateUIRootNode(UITier uiTier)
        {
            GameObject Global = GameObject.Find("Global");
            if (Global == null)
                Global = new GameObject("Global");
            GameObject uiRoot = GameObject.Find("UIRoot");
            if (uiRoot == null)
            {
                uiRoot = new GameObject("UIRoot");
                uiRoot.AddComponent<EventSystem>();
                uiRoot.AddComponent<StandaloneInputModule>();
            }

            GameObject uiRootNode = GameObject.Find($"{uiTier}_Root");
            if (uiRootNode != null) return;
            uiRootNode = new GameObject($"{uiTier}_Root");
            uiRoot.transform.SetParent(Global.transform);
            uiRootNode.transform.SetParent(uiRoot.transform);

            //创建基础组件
            uiRootNode.AddComponent<RectTransform>();
            uiRootNode.AddComponent<GraphicRaycaster>();
            Canvas canvas = uiRootNode.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = (int)uiTier;
            CanvasScaler canvasScaler = uiRootNode.AddComponent<CanvasScaler>();

            GameFrame.TEventHandler.Push(new GameFrame.CreatorUIRootControl_Event()
            {
                UITier = uiTier,
                CanvasScaler = canvasScaler,
                Canvas = canvas,
            });
        }

        string CreateUIRootNodeScript(UITier uiTier)
        {
            //获取数据模板
            string templatePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
                "GameFrameConfig/UI/UIRootNodeHandelTemplate.txt");
            string template = File.ReadAllText(templatePath);
            template = template.Replace("#VariableName#", $"{uiTier}_Root");
            return template;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        void SaveFrameData()
        {
            UIFrameWindData uiFrameWindData = new UIFrameWindData()
            {
                ScriptSavePath = ScriptSavePath,
                UIDlgKey = UIDlgKey,
                UIControlKey = UIControlKey,
                UILoadMode = UILoadMode,
                UIPrefabAssetPath = UIPrefabAssetPath,
                UIScriptDefaultTier = UIScriptDefaultTier,
                ICSEWTOverwrite = ICSEWTOverwrite,
            };
            UIFrameWindDataUtility.SetUIFrameWindData(uiFrameWindData);
            Log.Debug("保存UI框架配置文件");
            AssetDatabase.Refresh();
        }

        static void CreateClass(string fileName, string data)
        {
            //创建文件夹
            string tempPath = Path.Combine(Application.dataPath, "Scripts/Code/Frame/UI");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            //写入文件
            string writePath = Path.Combine(tempPath, fileName + ".cs");
            if (!File.Exists(writePath))
            {
                File.WriteAllText(writePath, data);
            }
            else
            {
                File.Delete(writePath);
                File.WriteAllText(writePath, data);
            }
        }
    }
}