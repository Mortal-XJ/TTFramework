// asmdef Version Defines, enabled when com.unity.addressables is imported.

#if UNITASK_ADDRESSABLE_SUPPORT

using Cysharp.Threading.Tasks.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Cysharp.Threading.Tasks
{
    public static class AddressablesAsyncExtensions
    {
#region AsyncOperationHandle

        // 将 AsyncOperationHandle 转换为 UniTask.Awaiter，使其可被 await
        public static UniTask.Awaiter GetAwaiter(this AsyncOperationHandle handle)
        {
            return ToUniTask(handle).GetAwaiter();
        }

        // 允许在异步操作期间使用 CancellationToken 进行取消
        public static UniTask WithCancellation(this AsyncOperationHandle handle, CancellationToken cancellationToken)
        {
            return ToUniTask(handle, cancellationToken: cancellationToken);
        }

        // 将 AsyncOperationHandle 转换为 UniTask，允许在异步代码中等待资源加载的完成
        public static UniTask ToUniTask(this AsyncOperationHandle handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled(cancellationToken);

            if (!handle.IsValid())
            {
                // 如果 handle 无效，则返回已完成的 UniTask
                // autoReleaseHandle:true handle 是无效的（立即内部 handle == null），因此返回已完成的任务。
                return UniTask.CompletedTask;
            }

            if (handle.IsDone)
            {
                // 如果异步操作已经完成
                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    // 如果操作失败，将异常包装为 UniTask 异常
                    return UniTask.FromException(handle.OperationException);
                }
                // 如果操作成功完成，返回已完成的 UniTask
                return UniTask.CompletedTask;
            }

            // 创建一个新的 UniTask，用于管理异步操作
            return new UniTask(AsyncOperationHandleConfiguredSource.Create(handle, timing, progress, cancellationToken, out var token), token);
        }

        // 用于等待 AsyncOperationHandle 的结构
        public struct AsyncOperationHandleAwaiter : ICriticalNotifyCompletion
        {
            AsyncOperationHandle handle;
            Action<AsyncOperationHandle> continuationAction;

            public AsyncOperationHandleAwaiter(AsyncOperationHandle handle)
            {
                this.handle = handle;
                this.continuationAction = null;
            }

            // 判断异步操作是否已完成
            public bool IsCompleted => handle.IsDone;

            // 获取异步操作的结果
            public void GetResult()
            {
                if (continuationAction != null)
                {
                    handle.Completed -= continuationAction;
                    continuationAction = null;
                }

                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    // 如果异步操作失败，捕获异常并抛出
                    var e = handle.OperationException;
                    handle = default;
                    ExceptionDispatchInfo.Capture(e).Throw();
                }

                var result = handle.Result;
                handle = default;
            }

            // 注册异步操作完成时的回调
            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            // 注册异步操作完成时的回调（不安全的版本）
            public void UnsafeOnCompleted(Action continuation)
            {
                Error.ThrowWhenContinuationIsAlreadyRegistered(continuationAction);
                continuationAction = PooledDelegate<AsyncOperationHandle>.Create(continuation);
                handle.Completed += continuationAction;
            }
        }

        // 异步操作的配置源，用于管理异步操作的状态
        sealed class AsyncOperationHandleConfiguredSource : IUniTaskSource, IPlayerLoopItem, ITaskPoolNode<AsyncOperationHandleConfiguredSource>
        {
            static TaskPool<AsyncOperationHandleConfiguredSource> pool;
            AsyncOperationHandleConfiguredSource nextNode;
            public ref AsyncOperationHandleConfiguredSource NextNode => ref nextNode;

            static AsyncOperationHandleConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(AsyncOperationHandleConfiguredSource), () => pool.Size);
            }

            readonly Action<AsyncOperationHandle> continuationAction;
            AsyncOperationHandle handle;
            CancellationToken cancellationToken;
            IProgress<float> progress;
            bool completed;

            UniTaskCompletionSourceCore<AsyncUnit> core;

            AsyncOperationHandleConfiguredSource()
            {
                continuationAction = Continuation;
            }

            // 创建一个新的 IUniTaskSource 实例，用于管理异步操作
            public static IUniTaskSource Create(AsyncOperationHandle handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    // 如果传入的 CancellationToken 已被取消，返回一个已取消的任务
                    return AutoResetUniTaskCompletionSource.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new AsyncOperationHandleConfiguredSource();
                }

                result.handle = handle;
                result.progress = progress;
                result.cancellationToken = cancellationToken;
                result.completed = false;

                // 跟踪活动任务以进行监视
                TaskTracker.TrackActiveTask(result, 3);

                // 在指定的 PlayerLoopTiming 中注册此操作，以便在 Unity 主循环中执行
                PlayerLoopHelper.AddAction(timing, result);

                // 注册异步操作完成时的回调
                handle.Completed += result.continuationAction;

                token = result.core.Version;
                return result;
            }

            // 异步操作完成时的回调方法
            void Continuation(AsyncOperationHandle _)
            {
                handle.Completed -= continuationAction;

                if (completed)
                {
                    TryReturn();
                }
                else
                {
                    completed = true;
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // 如果传入的 CancellationToken 已被取消，设置任务为已取消状态
                        core.TrySetCanceled(cancellationToken);
                    }
                    else if (handle.Status == AsyncOperationStatus.Failed)
                    {
                        // 如果异步操作失败，设置任务为异常状态，并将异常包装为 UniTask 异常
                        core.TrySetException(handle.OperationException);
                    }
                    else
                    {
                        // 如果异步操作成功完成，设置任务为已完成状态
                        core.TrySetResult(AsyncUnit.Default);
                    }
                }
            }

            // 获取异步操作的结果（IUniTaskSource 接口的方法）
            public void GetResult(short token)
            {
                core.GetResult(token);
            }

            // 获取任务的状态（IUniTaskSource 接口的方法）
            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            // 获取任务的状态（不安全的版本）
            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            // 注册异步操作完成时的回调（IUniTaskSource 接口的方法）
            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            // 在主循环中继续执行任务
            public bool MoveNext()
            {
                if (completed)
                {
                    TryReturn();
                    return false;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    completed = true;
                    // 如果传入的 CancellationToken 已被取消，设置任务为已取消状态
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (progress != null && handle.IsValid())
                {
                    // 如果有进度报告回调，并且异步操作仍然有效，报告进度
                    progress.Report(handle.PercentComplete);
                }

                return true;
            }

            // 尝试将对象返回到池中以进行重用
            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                handle = default;
                progress = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

#endregion

#region AsyncOperationHandle_T

        // 将 AsyncOperationHandle<T> 转换为 UniTask<T>.Awaiter，使其可被 await
        public static UniTask<T>.Awaiter GetAwaiter<T>(this AsyncOperationHandle<T> handle)
        {
            return ToUniTask(handle).GetAwaiter();
        }

        // 允许在异步操作期间使用 CancellationToken 进行取消
        public static UniTask<T> WithCancellation<T>(this AsyncOperationHandle<T> handle, CancellationToken cancellationToken)
        {
            return ToUniTask(handle, cancellationToken: cancellationToken);
        }

        // 将 AsyncOperationHandle<T> 转换为 UniTask<T>，允许在异步代码中等待资源加载的完成
        public static UniTask<T> ToUniTask<T>(this AsyncOperationHandle<T> handle, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.IsCancellationRequested) return UniTask.FromCanceled<T>(cancellationToken);

            if (!handle.IsValid())
            {
                // 如果 handle 无效，则抛出异常
                throw new Exception("Attempting to use an invalid operation handle");
            }

            if (handle.IsDone)
            {
                // 如果异步操作已经完成
                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    // 如果操作失败，将异常包装为 UniTask 异常
                    return UniTask.FromException<T>(handle.OperationException);
                }
                // 如果操作成功完成，返回包含结果的 UniTask
                return UniTask.FromResult(handle.Result);
            }

            // 创建一个新的 UniTask<T>，用于管理异步操作
            return new UniTask<T>(AsyncOperationHandleConfiguredSource<T>.Create(handle, timing, progress, cancellationToken, out var token), token);
        }

        // AsyncOperationHandle<T> 的配置源，用于管理异步操作的状态
        sealed class AsyncOperationHandleConfiguredSource<T> : IUniTaskSource<T>, IPlayerLoopItem, ITaskPoolNode<AsyncOperationHandleConfiguredSource<T>>
        {
            static TaskPool<AsyncOperationHandleConfiguredSource<T>> pool;
            AsyncOperationHandleConfiguredSource<T> nextNode;
            public ref AsyncOperationHandleConfiguredSource<T> NextNode => ref nextNode;

            static AsyncOperationHandleConfiguredSource()
            {
                TaskPool.RegisterSizeGetter(typeof(AsyncOperationHandleConfiguredSource<T>), () => pool.Size);
            }

            readonly Action<AsyncOperationHandle<T>> continuationAction;
            AsyncOperationHandle<T> handle;
            CancellationToken cancellationToken;
            IProgress<float> progress;
            bool completed;

            UniTaskCompletionSourceCore<T> core;

            AsyncOperationHandleConfiguredSource()
            {
                continuationAction = Continuation;
            }

            // 创建一个新的 IUniTaskSource<T> 实例，用于管理异步操作
            public static IUniTaskSource<T> Create(AsyncOperationHandle<T> handle, PlayerLoopTiming timing, IProgress<float> progress, CancellationToken cancellationToken, out short token)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    // 如果传入的 CancellationToken 已被取消，返回一个已取消的任务
                    return AutoResetUniTaskCompletionSource<T>.CreateFromCanceled(cancellationToken, out token);
                }

                if (!pool.TryPop(out var result))
                {
                    result = new AsyncOperationHandleConfiguredSource<T>();
                }

                result.handle = handle;
                result.cancellationToken = cancellationToken;
                result.completed = false;
                result.progress = progress;

                // 跟踪活动任务以进行监视
                TaskTracker.TrackActiveTask(result, 3);

                // 在指定的 PlayerLoopTiming 中注册此操作，以便在 Unity 主循环中执行
                PlayerLoopHelper.AddAction(timing, result);

                // 注册异步操作完成时的回调
                handle.Completed += result.continuationAction;

                token = result.core.Version;
                return result;
            }

            // 异步操作完成时的回调方法
            void Continuation(AsyncOperationHandle<T> argHandle)
            {
                handle.Completed -= continuationAction;

                if (completed)
                {
                    TryReturn();
                }
                else
                {
                    completed = true;
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // 如果传入的 CancellationToken 已被取消，设置任务为已取消状态
                        core.TrySetCanceled(cancellationToken);
                    }
                    else if (argHandle.Status == AsyncOperationStatus.Failed)
                    {
                        // 如果异步操作失败，设置任务为异常状态，并将异常包装为 UniTask 异常
                        core.TrySetException(argHandle.OperationException);
                    }
                    else
                    {
                        // 如果异步操作成功完成，设置任务为已完成状态，并包含结果
                        core.TrySetResult(argHandle.Result);
                    }
                }
            }

            // 获取异步操作的结果（IUniTaskSource<T> 接口的方法）
            public T GetResult(short token)
            {
                return core.GetResult(token);
            }

            // 获取异步操作的结果（IUniTaskSource 接口的方法）
            void IUniTaskSource.GetResult(short token)
            {
                GetResult(token);
            }

            // 获取任务的状态（IUniTaskSource 接口的方法）
            public UniTaskStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            // 获取任务的状态（不安全的版本）
            public UniTaskStatus UnsafeGetStatus()
            {
                return core.UnsafeGetStatus();
            }

            // 注册异步操作完成时的回调（IUniTaskSource 接口的方法）
            public void OnCompleted(Action<object> continuation, object state, short token)
            {
                core.OnCompleted(continuation, state, token);
            }

            // 在主循环中继续执行任务
            public bool MoveNext()
            {
                if (completed)
                {
                    TryReturn();
                    return false;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    completed = true;
                    // 如果传入的 CancellationToken 已被取消，设置任务为已取消状态
                    core.TrySetCanceled(cancellationToken);
                    return false;
                }

                if (progress != null && handle.IsValid())
                {
                    // 如果有进度报告回调，并且异步操作仍然有效，报告进度
                    progress.Report(handle.PercentComplete);
                }

                return true;
            }

            // 尝试将对象返回到池中以进行重用
            bool TryReturn()
            {
                TaskTracker.RemoveTracking(this);
                core.Reset();
                handle = default;
                progress = default;
                cancellationToken = default;
                return pool.TryPush(this);
            }
        }

#endregion
    }
}

#endif
