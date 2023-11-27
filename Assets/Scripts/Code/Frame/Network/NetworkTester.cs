using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public class NetworkTester 
    {
        public static async UniTask<bool> TestNetworkAsync()
        {
            string address = Network_Config.Ping_IP;
            Ping ping = new Ping(address);
            
            // 等待Ping的完成
            while (!ping.isDone)
            {
                await UniTask.Delay(100);
            }
            // 检查Ping是否成功
            if (ping.time != -1)
                return true;
            else
            {
                Log.Debug($"Ping to {address} failed.",Color.red);
                TEventHandler.Push(new NetworkAbnormal_Network_Event());//通知观察者网络异常
                return false;
            }
        }
    }
}