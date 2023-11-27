//----------------------
// Developer 
// Date  -  - 
// Script Overview 
//----------------------

using Cysharp.Threading.Tasks;
using GameFrame;
using UnityEngine.AddressableAssets;

namespace GameLogic
{
    [UnityEngine.DisallowMultipleComponent]
    public partial class TDemoImage:TDemoImage_Mono,GameFrame.IUIShowWind
    {
        public override void OnBindUIEvent()
        {
            _selfView.TCTestSwButton_Button.onClick.AddListener((() =>
            {
                Hot().Forget();
            }));
        }

        public override void OnRelieveBindUIEvent()
        {
            _selfView.TCTestSwButton_Button.onClick.RemoveAllListeners();
        }

        async UniTaskVoid Hot()
        {
            string name = _selfModel.isNoe ? "u=112G" : "u=365164386JPEG";
            _selfModel.isNoe = !_selfModel.isNoe;
            UnityEngine.UI.Image image = _selfView.SelfTransform.GetComponent<UnityEngine.UI.Image>();
            image.sprite = await Addressables.LoadAssetAsync<UnityEngine.Sprite>(name);
            
        }
        

        public override void OnShow()
        {
        }

        public override void OnHide()
        {
            
        }
    }
}