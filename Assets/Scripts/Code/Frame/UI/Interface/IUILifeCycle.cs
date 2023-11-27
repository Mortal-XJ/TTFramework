using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// UI生命周期
    /// </summary>
    public interface IUILifeCycle
    {
        // -----------------------------------------------
        // 执行顺序从上往下依次执行 
        // 注意 OnHide和OnReclaim的区别在于 
        // --OnReclaim回收到内存
        // --OnHide只是隐藏这个UI并不会消失在Hierarchy面板中
        // -----------------------------------------------

        UITier GetUITier();
        
        /// <summary>
        /// UI管理器用来初始化的
        /// </summary>
        void OnInit();
        /// <summary>
        /// 从硬盘加载到内存的时候调用
        /// </summary>
        void Onload();
        /// <summary>
        ///  从内存卸载的时候调用
        /// </summary>
        void OnUnload();
        /// <summary>
        /// 绑定UI控件使用
        /// </summary>
        void OnBindUIEvent();
        /// <summary>
        /// 解除绑定UI控件使用
        /// </summary>
        void OnRelieveBindUIEvent();
        /// <summary>
        /// UI显示的时候调用
        /// </summary>
        void OnShow();
        /// <summary>
        /// UI隐藏但不回收到管理器
        /// </summary>
        void OnHide();

    }
}