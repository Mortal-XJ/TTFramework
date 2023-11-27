using UnityEngine;

namespace GameFrame
{
    public class UIRootNodeHandel:Singleton<UIRootNodeHandel>
    {
        private Transform _uiRootNode;
        public Transform UIRootNode
        {
            get
            {
                if (_uiRootNode != null)
                    return _uiRootNode;
                _uiRootNode = GameObject.Find("UIRoot").transform;
                if (_uiRootNode == null)
                    throw new System.Exception("获取UI根节点失败 UIRoot");
                return _uiRootNode;
            }
        }
                
        string Ordinary_RootName = "Ordinary_Root";
        private Transform _Ordinary_Root;
        public Transform Ordinary_Root
        {
            get
            {
                if (_Ordinary_Root != null)
                    return _Ordinary_Root;
                Transform temp = UIRootNode.Find(Ordinary_RootName);
                if (temp == null)
                {
                    Log.Warning($"{this.GetType().Name}: Get { Ordinary_RootName} Fail", Color.red);
                    return null;
                }

                _Ordinary_Root = temp;
                return _Ordinary_Root;
            }
        }        
        string Fixed_RootName = "Fixed_Root";
        private Transform _Fixed_Root;
        public Transform Fixed_Root
        {
            get
            {
                if (_Fixed_Root != null)
                    return _Fixed_Root;
                Transform temp = UIRootNode.Find(Fixed_RootName);
                if (temp == null)
                {
                    Log.Warning($"{this.GetType().Name}: Get { Fixed_RootName} Fail", Color.red);
                    return null;
                }

                _Fixed_Root = temp;
                return _Fixed_Root;
            }
        }        
        string Eject_RootName = "Eject_Root";
        private Transform _Eject_Root;
        public Transform Eject_Root
        {
            get
            {
                if (_Eject_Root != null)
                    return _Eject_Root;
                Transform temp = UIRootNode.Find(Eject_RootName);
                if (temp == null)
                {
                    Log.Warning($"{this.GetType().Name}: Get { Eject_RootName} Fail", Color.red);
                    return null;
                }

                _Eject_Root = temp;
                return _Eject_Root;
            }
        }

        /// <summary>
        /// 根据层级获取Transform
        /// </summary>
        /// <param name="uiTier"></param>
        /// <returns></returns>
        public Transform GetTransform(UITier uiTier)
        {
            Transform trans = null;
            switch (uiTier+"_Root")
            {
                
                    case "Ordinary_Root":
                        trans = Ordinary_Root;
                    break;
                    case "Fixed_Root":
                        trans = Fixed_Root;
                    break;
                    case "Eject_Root":
                        trans = Eject_Root;
                    break;
            }

            return trans;
        }
    }
}