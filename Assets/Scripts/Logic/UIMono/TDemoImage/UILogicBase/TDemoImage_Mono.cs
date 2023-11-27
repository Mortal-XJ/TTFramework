//----------------------
// Developer 
// Date  -  - 
// Script Overview 
//----------------------
using GameFrame;

namespace GameLogic
{
    [UnityEngine.RequireComponent(typeof(TDemoImage_Model))]
    public partial class TDemoImage_Mono:UnityEngine.MonoBehaviour,IUILifeCycle
    {
        public virtual UITier GetUITier()
        {
            return  UITier.Ordinary;
        }

        protected TDemoImage_Component _selfView;
        protected TDemoImage_Model _selfModel;
        /// <summary>
        /// UI管理器用来初始化的
        /// </summary>
        public virtual void OnInit()
        {
            _selfView = new TDemoImage_Component(); 
            _selfModel = gameObject.GetComponent<TDemoImage_Model>();
            if (_selfModel == null)
                _selfModel = gameObject.AddComponent<TDemoImage_Model>();
            _selfView.BindRootTransform(gameObject.transform);
        }

         /// <summary>
        /// 从硬盘加载到内存的时候调用
        /// </summary>
        public virtual void Onload()
        {
        }
        /// <summary>
        /// 从内存卸载的时候调用
        /// </summary>
        public virtual void OnUnload()
        {
        }

        /// <summary>
        /// 绑定UI控件使用
        /// </summary>
        public virtual void OnBindUIEvent()
        {
           
        }

        /// <summary>
        /// 解除绑定UI控件使用
        /// </summary>
        public virtual void OnRelieveBindUIEvent()
        {
            
        }
        /// <summary>
        /// UI显示的时候调用
        /// </summary>
        public virtual void OnShow()
        {
           
        }
        /// <summary>
        /// UI隐藏但不回收到管理器
        /// </summary>
        public virtual void OnHide()
        {
          
        }
    }
}