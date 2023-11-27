//----------------------
// Developer Mortal
// Date  -  - 
// Script Overview 
//----------------------
using UnityEngine;
using GameFrame;

namespace GameLogic
{
    public partial class TDemoImage_Component : GameFrame.IUIRelease
    {
        /// <summary>
        /// UIRoot节点
        /// </summary>
        private Transform _selfTransform;
        public Transform SelfTransform => _selfTransform;

        /// <summary>
        /// 绑定根节点
        /// </summary>
        /// <param name="transform"></param>
        public void BindRootTransform(Transform transform)
        {
            _selfTransform = transform;
        }

                private UnityEngine.RectTransform _TCTestSwButton_RectTransform;
        public UnityEngine.RectTransform TCTestSwButton_RectTransform
        {
            get
            {
                if (SelfTransform == null)
                {
                    Log.Warning($"{this.GetType().Name}: SelfTransform EqualTo Null", Color.red);
                    return null;
                }

                if (_TCTestSwButton_RectTransform != null)
                    return _TCTestSwButton_RectTransform;
                UnityEngine.RectTransform temp = UIHandle.GetControl<UnityEngine.RectTransform>(SelfTransform, "TCTestSwButton");
                if (temp == null)
                {
                    Log.Warning($"{this.GetType().Name}: Get UnityEngine.RectTransform Fail", Color.red);
                    return null;
                }

                _TCTestSwButton_RectTransform = temp;
                return _TCTestSwButton_RectTransform;
            }
        }
        private UnityEngine.CanvasRenderer _TCTestSwButton_CanvasRenderer;
        public UnityEngine.CanvasRenderer TCTestSwButton_CanvasRenderer
        {
            get
            {
                if (SelfTransform == null)
                {
                    Log.Warning($"{this.GetType().Name}: SelfTransform EqualTo Null", Color.red);
                    return null;
                }

                if (_TCTestSwButton_CanvasRenderer != null)
                    return _TCTestSwButton_CanvasRenderer;
                UnityEngine.CanvasRenderer temp = UIHandle.GetControl<UnityEngine.CanvasRenderer>(SelfTransform, "TCTestSwButton");
                if (temp == null)
                {
                    Log.Warning($"{this.GetType().Name}: Get UnityEngine.CanvasRenderer Fail", Color.red);
                    return null;
                }

                _TCTestSwButton_CanvasRenderer = temp;
                return _TCTestSwButton_CanvasRenderer;
            }
        }
        private UnityEngine.UI.Image _TCTestSwButton_Image;
        public UnityEngine.UI.Image TCTestSwButton_Image
        {
            get
            {
                if (SelfTransform == null)
                {
                    Log.Warning($"{this.GetType().Name}: SelfTransform EqualTo Null", Color.red);
                    return null;
                }

                if (_TCTestSwButton_Image != null)
                    return _TCTestSwButton_Image;
                UnityEngine.UI.Image temp = UIHandle.GetControl<UnityEngine.UI.Image>(SelfTransform, "TCTestSwButton");
                if (temp == null)
                {
                    Log.Warning($"{this.GetType().Name}: Get UnityEngine.UI.Image Fail", Color.red);
                    return null;
                }

                _TCTestSwButton_Image = temp;
                return _TCTestSwButton_Image;
            }
        }
        private UnityEngine.UI.Button _TCTestSwButton_Button;
        public UnityEngine.UI.Button TCTestSwButton_Button
        {
            get
            {
                if (SelfTransform == null)
                {
                    Log.Warning($"{this.GetType().Name}: SelfTransform EqualTo Null", Color.red);
                    return null;
                }

                if (_TCTestSwButton_Button != null)
                    return _TCTestSwButton_Button;
                UnityEngine.UI.Button temp = UIHandle.GetControl<UnityEngine.UI.Button>(SelfTransform, "TCTestSwButton");
                if (temp == null)
                {
                    Log.Warning($"{this.GetType().Name}: Get UnityEngine.UI.Button Fail", Color.red);
                    return null;
                }

                _TCTestSwButton_Button = temp;
                return _TCTestSwButton_Button;
            }
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        public void Release()
        {
            this._TCTestSwButton_RectTransform  = null; 
            this._TCTestSwButton_CanvasRenderer  = null; 
            this._TCTestSwButton_Image  = null; 
            this._TCTestSwButton_Button  = null; 
            
            _selfTransform = null;
        }
    }
}