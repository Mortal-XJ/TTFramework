using cfg;
using Luban;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameFrame
{
    public static class ExcelHandler
    {
        public static Tables _tables;
        static ExcelHandler()
        {
            _tables = new cfg.Tables(LoadByteBufAsync);
        }
        private static ByteBuf LoadByteBufAsync(string file)
        {

            TextAsset asset =  Addressables.LoadAssetAsync<TextAsset>(file).WaitForCompletion();
            if (asset == null)
            {
                Log.Error($"ExcelHandler.LoadByteBufAsync Load-{file} Failed");
                return null;
            }
            return new ByteBuf(asset.bytes);
        }
    }

}