using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Internal;
using UnityEngine;

namespace GameFrame
{
    internal interface IUEvent<T>
    {
        abstract void Handler(T tEvent);
    }

    [TEventHandler]
    public abstract class TEvent<T> : IUEvent<T>
    {
        public abstract void Handler(T tEvent);
    }
    [TEventHandler]
    public abstract class TEventAsync<T> : IUEvent<T> where T : struct
    {
        public void Handler(T tEvent)
        {
            this.HandlerAsync(tEvent).Forget();
        }

        public virtual async UniTaskVoid HandlerAsync(T tEvent)
        {
            await UniTask.CompletedTask;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TEventHandlerAttribute: Attribute
    {
    }

    public static class TEventHandler
    {
        private static ConcurrentDictionary<string, List<Type>> _eventCacheCeDic;

        static  TEventHandler()
        {
            _eventCacheCeDic = new ConcurrentDictionary<string, List<Type>>();
            var tempEventClass = Assembly.GetExecutingAssembly().GetTypes().Where(Sift);
            foreach (var type in tempEventClass)
            {
                string tempArgumentsName = type.BaseType.GetGenericArguments()[0]?.Name;
                List<Type> tempEvent;
                if (_eventCacheCeDic.TryGetValue(tempArgumentsName, out tempEvent))
                {
                    tempEvent.Add(type);
                    continue;
                }
                tempEvent = new List<Type>();
                tempEvent.Add(type);
                _eventCacheCeDic.TryAdd(tempArgumentsName, tempEvent);
            }
        }

        private static bool Sift(Type arg)
        {
            if (arg != typeof(TEventAsync<>) && arg != typeof(TEvent<>) &&
                arg.GetCustomAttributes(typeof(TEventHandlerAttribute), true).Length > 0)
                return true;
            return false;
        }

        public static void Push<T>(T t) where T : struct
        {
            List<Type> tempEvent;
            IUEvent<T> eventClass;
            if (!_eventCacheCeDic.TryGetValue(typeof(T).Name, out tempEvent)) return;
            for (int i = 0; i < tempEvent.Count; i++)
            {
                eventClass = Activator.CreateInstance(tempEvent[i]) as IUEvent<T>;
                Error.ThrowArgumentNullException(eventClass, nameof(eventClass));
                eventClass.Handler(t);
            }
        }

              #region 全局事件

        // 使用字典存储事件名和相应的Action
        private static Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

        // 添加事件监听器
        public static void AddEventListener<T>(string eventName, Action<T> listener)
        {
            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = listener;
            }
            else
            {
                eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], listener);
            }
        }

        // 移除事件监听器
        public static void RemoveEventListener<T>(string eventName, Action<T> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], listener);

                // 如果没有监听器了，从字典中移除该事件
                if (eventDictionary[eventName] == null)
                {
                    eventDictionary.Remove(eventName);
                }
            }
        }

        // 触发事件
        public static void TriggerEvent<T>(string eventName, T eventData)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                // 获取事件对应的委托
                Delegate action = eventDictionary[eventName];

                // 调用所有监听器的Action
                if (action != null && action is Action<T>)
                {
                    (action as Action<T>).Invoke(eventData);
                }
            }
        }

        #endregion
       
    }
   
}
