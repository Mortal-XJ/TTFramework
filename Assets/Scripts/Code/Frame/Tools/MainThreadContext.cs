using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public class MainThreadContext : SingletonMono<MainThreadContext>
    {
        private ConcurrentQueue<Action> m_ToBeExecutedn = new ConcurrentQueue<Action>();

        private bool isUpdate;

        public override void Init()
        {
            isUpdate = true;
            ThreadUpdateAsync().Forget();
        }

        public void Push(Action action)
        {
            m_ToBeExecutedn.Enqueue(action);
        }

        async UniTaskVoid ThreadUpdateAsync()
        {
            while (isUpdate)
            {
                if (m_ToBeExecutedn.Count > 0)
                {
                    m_ToBeExecutedn.TryDequeue(out Action action);
                    action?.Invoke();
                }

                await UniTask.Yield();
            }
        }

        private void OnDestroy()
        {
            isUpdate = false;
        }
    }
}