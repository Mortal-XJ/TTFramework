using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public interface IAwaiter
    {
    }

    public static class OperationAwaiter
    {
        private static Dictionary<string, List<UniTaskCompletionSource<IAwaiter>>> _tasksListDictionary;

        static OperationAwaiter()
        {
            _tasksListDictionary = new Dictionary<string, List<UniTaskCompletionSource<IAwaiter>>>();
        }

        public static void Finish<T>(T result) where T : IAwaiter
        {
            if (!_tasksListDictionary.TryGetValue(nameof(T), out List<UniTaskCompletionSource<IAwaiter>> sources))
                return;

            foreach (var source in sources)
            {
                source.TrySetResult(result);
            }

            _tasksListDictionary.Remove(nameof(T));
        }

        public static async UniTask<T> WaitAsync<T>() where T : class, IAwaiter
        {
            if (!_tasksListDictionary.TryGetValue(nameof(T), out List<UniTaskCompletionSource<IAwaiter>> completionSources))
            {
                completionSources = new List<UniTaskCompletionSource<IAwaiter>>();
                _tasksListDictionary.Add(nameof(T), completionSources);
            }

            var newCompletionSource = new UniTaskCompletionSource<IAwaiter>();
            completionSources.Add(newCompletionSource);

            return await newCompletionSource.Task as T;
        }
    }
}