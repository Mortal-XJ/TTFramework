//----------------------
// Developer Mortal
// Date 2023 - 10 - 23 
// Script Overview 
//----------------------

using UnityEngine.UI;

namespace GameFrame
{
    public enum UITier
    {
        /// <summary>
        /// 普通层级 todo 必须有0层级
        /// </summary>
        Ordinary = 0,

        /// <summary>
        /// 固定层级
        /// </summary>
        Fixed = 10,

        /// <summary>
        /// 弹窗层级
        /// </summary>
        Eject = 20,
    }

    /// <summary>
    /// 每当创建一个根层级 这里就会收到回调 可以在这里面做个性化数据指定
    /// </summary>
    class UITierLevelInfoSet : TEvent<CreatorUIRootControl_Event>
    {
        public override void Handler(CreatorUIRootControl_Event tEvent)
        {
            UnityEngine.UI.CanvasScaler scaler = tEvent.CanvasScaler;
            UnityEngine.Canvas canvas = tEvent.Canvas;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new UnityEngine.Vector2(1920, 1080);
            
            switch (tEvent.UITier)
            {
                case UITier.Ordinary:
                    break;
                case UITier.Fixed:
                    break;
                case UITier.Eject:
                    break;
            }
        }
    }
}