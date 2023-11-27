using Cysharp.Threading.Tasks;
namespace GameFrame
{
    public static class Game
    {
        public static async UniTask InitAsync()
        {
            //初始化
            MainThreadContext.Instance.Init();
            AudioHandler.Instance.Init();
            await UIManager.Instance.InitAsync();
        }
    }
}