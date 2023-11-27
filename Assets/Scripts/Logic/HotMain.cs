using GameFrame;
using Cysharp.Threading.Tasks;
using GameLogic;

public static class HotMain 
{
    // Start is called before the first frame update
    public static void Start()
    {
        StartAsync().Forget();
    }
    static async UniTask StartAsync()
    {
        await Game.InitAsync();

        #region Test

        UIManager.Instance.PushWindAsync<TDemoImage>();
        TEventHandler.Push(new GameFrame.StartGame_Event());
        #endregion
    }
}
