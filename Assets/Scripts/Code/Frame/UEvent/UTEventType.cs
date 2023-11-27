//----------------------
// Developer Mortal
// Date 2023 - 9 - 16 
// Script Overview 以类为单位的事件系统配置文件
// NamingConvention 后面要加 _Event
//----------------------

namespace GameFrame
{
    #region Editor

    public struct CreatorUIRootControl_Event
    {
        public GameFrame.UITier UITier;
        public UnityEngine.UI.CanvasScaler CanvasScaler;
        public UnityEngine.Canvas Canvas;
    }

    #endregion
    public struct StartGame_Event
    {
    }

    /// <summary>
    /// 场景玩成加载时
    /// </summary>
    public struct TheSceneFinishLoading_Event
    {
    }

    #region Network
    /// <summary>
    /// 网络异常
    /// </summary>
    public struct NetworkAbnormal_Network_Event
    {
    }
    

    #endregion
}