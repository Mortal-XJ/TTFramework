//----------------------
// Developer Mortal
// Date 2023 - 10 - 23 
// Script Overview 
//----------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public enum UIUnitState
    {
        LOAD, //刚从硬盘加载出来
        UIShow, //UI正在显示
        UIHide, //UI正在隐藏
        UILinkedList, //UI正在链表中
    }

    public class UIUnit : IUIRelease
    {
        //上下节点
        public UIUnit NextNode;
        public UIUnit PreviousNode;

        public IUILifeCycle LifeCycle;

        /// <summary>
        /// 当前状态
        /// </summary>
        public UIUnitState MyState = UIUnitState.LOAD;

        /// <summary>
        /// 当前对象
        /// </summary>
        public GameObject UIGO;

        public UIUnit(GameObject uigo, IUILifeCycle uiLifeCycle)
        {
            this.UIGO = uigo;
            this.LifeCycle = uiLifeCycle;
        }

        public void Release()
        {
            MyState = 0;
            UIGO = null;
        }
    }

    public class UIManager : SingletonMono<UIManager>
    {
        /// <summary>
        /// Load出来的UI界面缓存
        /// </summary>
        private ConcurrentDictionary<string, UIUnit> _loadCacheDic = new ConcurrentDictionary<string, UIUnit>();

        /// <summary>
        /// 记录每个层级最顶部窗口
        /// </summary>
        private Dictionary<UITier, UIUnit> _uiUnitTopDic = new Dictionary<UITier, UIUnit>();


        /// <summary>
        /// UI框架配置数据
        /// </summary>
        private UIFrameWindData UIiFrameData;


        private UIUnit _uiUnitHandeNode;
        private UIUnit _uiUnitTailNode;
        public async UniTask InitAsync()
        {
            UIiFrameData = await UIFrameWindDataUtility.GetUIFrameWindDatsAsync();
        }

        // 添加一个节点到尾部
        void AddLinkedListNode(UIUnit uiUnit)
        {
            if (uiUnit.MyState == UIUnitState.UILinkedList) return;

            if (_uiUnitHandeNode == null)
            {
                //没有初始节点就把自己作为初始节点
                _uiUnitHandeNode = uiUnit;
                _uiUnitTailNode = uiUnit;
            }
            else
            {
                uiUnit.PreviousNode = _uiUnitTailNode; //自己的上个节点设置为尾部节点
                _uiUnitTailNode.NextNode = uiUnit; //尾部的下一个节点设置为自己
                _uiUnitTailNode = uiUnit; //自己等于尾部
            }

            uiUnit.MyState = UIUnitState.UILinkedList;
        }

        // 移除指定节点
        bool RemoveLinkedListNodeTarget(UIUnit uiUnit)
        {
            if (uiUnit.MyState != UIUnitState.UILinkedList || _uiUnitHandeNode == null) return false;

            //自己是头节点
            if (uiUnit == _uiUnitHandeNode)
            {
                _uiUnitHandeNode = _uiUnitHandeNode.NextNode; //将头节点设为自己的下一个节点
                _uiUnitHandeNode.PreviousNode = null;
                uiUnit.NextNode = null;
                uiUnit.PreviousNode = null;
                uiUnit.MyState = UIUnitState.UIHide;
                return true;
            }

            //自己是尾部节点
            if (uiUnit == _uiUnitTailNode)
            {
                _uiUnitTailNode = _uiUnitTailNode.PreviousNode; //将头结点设为尾部节点
                _uiUnitTailNode.NextNode = null;
                uiUnit.NextNode = null;
                uiUnit.PreviousNode = null;
                uiUnit.MyState = UIUnitState.UIHide;
                return true;
            }

            //剔除自己
            uiUnit.NextNode.PreviousNode = uiUnit.PreviousNode;
            uiUnit.PreviousNode.NextNode = uiUnit.NextNode;
            uiUnit.NextNode = null;
            uiUnit.PreviousNode = null;
            uiUnit.MyState = UIUnitState.UIHide;
            return true;
        }

        // 移除尾部节点
        bool RemoveEndLinkedListNode(out UIUnit uiUnit)
        {
            uiUnit = default;

            if (_uiUnitTailNode == null) return false;

            uiUnit = _uiUnitTailNode;
            _uiUnitTailNode = _uiUnitTailNode.PreviousNode; //交换节点

            if (_uiUnitTailNode != null)
            {
                _uiUnitTailNode.NextNode = null;
            }
            else
            {
                _uiUnitHandeNode = null; // 链表为空
            }

            uiUnit.NextNode = null;
            uiUnit.PreviousNode = null;
            uiUnit.MyState = UIUnitState.UIHide;

            return true;
        }

        public void MoveNodeToTail(UIUnit uiUnit)
        {
            if (uiUnit == null || uiUnit == _uiUnitTailNode || uiUnit.MyState != UIUnitState.UILinkedList)
            {
                return;
            }

            // 从链表中移除节点
            if (uiUnit == _uiUnitHandeNode)
            {
                _uiUnitHandeNode = uiUnit.NextNode;
                _uiUnitHandeNode.PreviousNode = null;
            }
            else
            {
                uiUnit.PreviousNode.NextNode = uiUnit.NextNode;
            }

            if (uiUnit == _uiUnitTailNode)
            {
                _uiUnitTailNode = uiUnit.PreviousNode;
            }
            else
            {
                uiUnit.NextNode.PreviousNode = uiUnit.PreviousNode;
            }

            // 将节点插入到链表的尾部
            uiUnit.PreviousNode = _uiUnitTailNode;
            _uiUnitTailNode.NextNode = uiUnit;
            _uiUnitTailNode = uiUnit;
            uiUnit.NextNode = null;
        }

        private void Start()
        {
            Array array = Enum.GetValues(typeof(UITier));
            for (int i = 0; i < array.Length; i++)
            {
                UITier temp = (UITier)array.GetValue(i);
                _uiUnitTopDic.Add(temp, null);
            }
        }


        /// <summary>
        /// 在UI框架中显示一个界面。
        /// </summary>
        /// <typeparam name="T">界面类型，必须实现GameFrame.IUILifeCycle接口。</typeparam>
        /// <returns>显示的界面实例。</returns>
        public async UniTask<T> ShowWindAsync<T>()
            where T : UnityEngine.Component, GameFrame.IUILifeCycle, GameFrame.IUIShowWind
        {
            T uiScript = null;
            if (!_loadCacheDic.TryGetValue(typeof(T).Name, out UIUnit uiUnit))
                uiScript = await LoadWindAsync<T>();
            uiUnit = _loadCacheDic[typeof(T).Name];

            //如果在链表中从链表取出
            if (uiUnit.MyState == UIUnitState.UILinkedList)
                RemoveLinkedListNodeTarget(uiUnit); //移除节点

            //进行移动
            uiUnit.MyState = UIUnitState.UIShow;
            MoveUIToTop(uiUnit);
            return uiScript;
        }

        /// <summary>
        /// 隐藏UI框架中的一个界面。
        /// </summary>
        /// <typeparam name="T">界面类型，必须实现GameFrame.IUILifeCycle接口。</typeparam>
        /// <returns>隐藏的界面实例。</returns>
        public void HideWind<T>() where T : UnityEngine.Component, GameFrame.IUILifeCycle, GameFrame.IUIShowWind
        {
            T uiScript = null;
            if (!_loadCacheDic.TryGetValue(typeof(T).Name, out UIUnit uiUnit))
            {
                Log.Warning($"不存在{typeof(T).Name}窗口 无法隐藏");
                return;
            }

            uiUnit = _loadCacheDic[typeof(T).Name];

            //如果该界面在列表中 则剔除
            if (uiUnit.MyState == UIUnitState.UILinkedList)
                RemoveLinkedListNodeTarget(uiUnit);
            uiUnit.MyState = UIUnitState.UIHide;
            //隐藏
            uiUnit.MyState = UIUnitState.UIHide;
            HideUIMoveBottom(uiUnit);
        }

        /// <summary>
        /// 在UI框架中推入一个界面。
        /// </summary>
        /// <typeparam name="T">界面类型，必须实现GameFrame.IUILifeCycle接口。</typeparam>
        /// <returns>推入的界面实例。</returns>
        public async UniTask<T> PushWindAsync<T>()
            where T : UnityEngine.Component, GameFrame.IUILifeCycle, GameFrame.IUIShowWind
        {
            T uiScript = null;
            if (!_loadCacheDic.TryGetValue(typeof(T).Name, out UIUnit uiUnit))
                uiScript = await LoadWindAsync<T>();
            uiUnit = _loadCacheDic[typeof(T).Name];

            //如果存在就进行置顶
            if (uiUnit.MyState == UIUnitState.UILinkedList)
            {
                MoveNodeToTail(uiUnit);
            }
            else
            {
                AddLinkedListNode(uiUnit);
            }


            //移动UI
            MoveUIToTop(uiUnit);
            return uiScript;
        }

        /// <summary>
        /// 从UI框架中弹出栈顶的一个界面。
        /// </summary>
        /// <typeparam name="T">界面类型，必须实现GameFrame.IUILifeCycle接口。</typeparam>
        /// <returns>弹出的界面实例。</returns>
        public void PopWind()
        {
            if (RemoveEndLinkedListNode(out UIUnit uiUnit))
            {
                HideUIMoveBottom(uiUnit);
                uiUnit.MyState = UIUnitState.UIHide;
            }
            else
            {
                Log.Warning("LinkedListsHaveNoUILeft", Color.gray);
            }
        }


        /// <summary>
        /// 加载UI框架中的一个界面。
        /// </summary>
        /// <typeparam name="T">界面类型，必须实现GameFrame.IUILifeCycle接口。</typeparam>
        /// <returns>加载的界面实例。</returns>
        public async UniTask<T> LoadWindAsync<T>()
            where T : UnityEngine.Component, GameFrame.IUILifeCycle, GameFrame.IUIShowWind
        {
            //检查缓存是否存在
            if (_loadCacheDic.TryGetValue(typeof(T).Name, out UIUnit value))
                return value.UIGO.GetComponent<T>();
            //LoadRes
            string path = "";
            if (UIiFrameData.UILoadMode == ResourceType.ADDRESSABLES)
                path = typeof(T).Name;
            else if (UIiFrameData.UILoadMode == ResourceType.RESOURCE)
                path = System.IO.Path.Combine(UIiFrameData.UIPrefabAssetPath, typeof(T).Name);
            GameObject gameObject =
                await ExplorerHandler.Instance.LoadResourceAsync<GameObject>(path, UIiFrameData.UILoadMode);
            Transform uiRootNode = UIRootNodeHandel.Instance.GetTransform(0);
            gameObject = GameObject.Instantiate(gameObject, uiRootNode);
            // //初始信息
            T temp = gameObject.GetComponent<T>();
            if (temp == null)
                temp = gameObject.AddComponent<T>();
            if (gameObject.GetComponent<CanvasGroup>() == null)
                gameObject.AddComponent<CanvasGroup>();
            gameObject.transform.SetParent(UIRootNodeHandel.Instance.GetTransform(temp.GetUITier()));

            UIUnit uiUnit = new UIUnit(gameObject, temp);
            uiUnit.UIGO = gameObject;

            temp.Onload();
            temp.OnInit();
            temp.OnBindUIEvent();
            //LoadCache
            _loadCacheDic.TryAdd(typeof(T).Name, uiUnit);
            return null;
        }

        /// <summary>
        /// 卸载UI框架中的一个界面。
        /// </summary>
        /// <typeparam name="T">界面类型，必须实现GameFrame.IUILifeCycle接口。</typeparam>
        /// <returns>卸载的界面实例。</returns>
        public void UnloadWind<T>() where T : GameFrame.IUILifeCycle, GameFrame.IUIShowWind
        {
            //检测是否存在缓存中
            if (!_loadCacheDic.TryRemove(typeof(T).Name, out UIUnit value))
            {
                Log.Warning($"NotFound{typeof(T).Name}UnableToProceedUnload", Color.green);
                return;
            }

            //执行卸载
            T t = value.UIGO.GetComponent<T>();
            t.OnRelieveBindUIEvent();
            t.OnUnload();
            GameObject.Destroy(value.UIGO);
            ExplorerHandler.Instance.UnloadAsset<GameObject>(value.UIGO); //尝试从内存中卸载
        }

        /// <summary>
        /// 查看指定层级中最顶部的一个界面
        /// </summary>
        /// <param name="tier">指定层级</param>
        /// <returns></returns>
        public GameObject PeekTopWind(UITier tier)
        {
            UIUnit temp = _uiUnitTopDic[tier];
            if (temp == null)
                return null;
            if (temp.MyState != UIUnitState.UIShow)
            {
                Transform root = UIRootNodeHandel.Instance.GetTransform(tier);
                for (int i = root.childCount - 1; i > -1; i--)
                {
                    Transform child = root.GetChild(i);
                    if (!_loadCacheDic.TryGetValue(child.gameObject.name, out UIUnit value)) continue;
                    if (value.MyState == UIUnitState.UIShow)
                    {
                        _uiUnitTopDic[tier] = value;
                        temp = value;
                        break;
                    }
                }
            }

            return temp.UIGO;
        }

        /// <summary>
        /// 将这个UI移动到最顶级 并显示它
        /// </summary>
        /// <param name="uiUnit"></param>
        void MoveUIToTop(UIUnit uiUnit)
        {
            UITier tier = uiUnit.UIGO.GetComponent<IUILifeCycle>().GetUITier();
            if (_uiUnitTopDic[tier] == uiUnit) return;
            uiUnit.LifeCycle.OnShow();
            CanvasGroup canvasGroup = uiUnit.UIGO.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            uiUnit.UIGO.transform.SetAsLastSibling();

            //设置该UI为最顶层
            _uiUnitTopDic[uiUnit.UIGO.GetComponent<IUILifeCycle>().GetUITier()] = uiUnit;
        }

        /// <summary>
        /// 隐藏这个UI并移动到最底部
        /// </summary>
        /// <param name="uiUnit"></param>
        void HideUIMoveBottom(UIUnit uiUnit)
        {
            UITier tier = uiUnit.UIGO.GetComponent<IUILifeCycle>().GetUITier();

            uiUnit.LifeCycle.OnHide();
            CanvasGroup canvasGroup = uiUnit.UIGO.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            uiUnit.UIGO.transform.SetAsFirstSibling();

            // 如果该 UI 单元是顶部 UI，则需要更新 _uiUnitTopDic
            if (_uiUnitTopDic.ContainsKey(tier) && _uiUnitTopDic[tier] == uiUnit)
            {
                // 遍历子物体找到当前显示的 UI 单元
                Transform root = UIRootNodeHandel.Instance.GetTransform(tier);
                for (int i = root.childCount - 1; i > -1; i--)
                {
                    Transform child = root.GetChild(i);
                    if (!_loadCacheDic.TryGetValue(child.gameObject.name, out UIUnit value)) continue;
                    if (value.MyState == UIUnitState.UIShow)
                    {
                        _uiUnitTopDic[tier] = value;
                        return;
                    }
                }

                _uiUnitTopDic[tier] = null;
            }
        }

        /// <summary>
        /// 根据脚本获取UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetUIScript<T>() where T : GameFrame.IUILifeCycle, GameFrame.IUIShowWind
        {
            if (_loadCacheDic.TryGetValue(typeof(T).Name, out UIUnit value))
                return value.UIGO.GetComponent<T>();
            else
            {
                Log.Error($"This {typeof(T).Name}UI Not Show");
                return default;
            }
        }
    }

    public static class UIHandle
    {
        public static T GetControl<T>(Transform selfTransform, string gameObjectName) where T : UnityEngine.Component
        {
            GameObject gameObject = selfTransform.Find(gameObjectName).gameObject;
            if (gameObject == null)
            {
                Log.Warning($"UIFrame UIHandle Get {gameObject} In {typeof(T).Namespace}.{typeof(T).Name} Failed",
                    Color.red);
                return null;
            }

            T temp = gameObject.GetComponent<T>();
            if (temp == null)
            {
                Log.Warning($"UIFrame UIHandle Get {gameObject} In {typeof(T).Namespace}.{typeof(T).Name} Failed",
                    Color.red);
                return null;
            }

            return temp;
        }
    }
}