using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameFrame
{
    public enum AADownloadOutcome_Type
    {
        //无需下载
        NoDownloadRequired,
        //下载完成
        DownloadIsComplete,

        //网络异常
        NetworkAbnormal,

        //其他错误
        Mistake
    }

    public class AddressablePreDownload
    {
        //预下载
        private List<string> _preDownload_List = new List<string>();

        private int _currentIndex;

        //下载总大小
        private long _totalDownloadSize;

        //下载总进度
        private float _downloadTheTotalProgress;

        private float DownloadTheTotalProgress
        {
            get => _downloadTheTotalProgress;
            set
            {
                DownloadTotalProgress?.Invoke(value);
                _downloadTheTotalProgress = value;
            }
        }

        public Action<float> DownloadTotalProgress;
        
        /// <summary>
        /// 添加预下载
        /// </summary>
        /// <param name="key"></param>
        public void AddPreDownload(string key)
        {
            _preDownload_List.Add(key);
        }

        public async UniTask<long> GetDownloadSize()
        {
            //获取总下载大小
            long size = 0;
            for (int i = 0; i < _preDownload_List.Count; i++)
            {
                size += await Addressables.GetDownloadSizeAsync(_preDownload_List[i]).ToUniTask();
            }

            _totalDownloadSize = size;
            return size;
        }

        //下载
        public async UniTask <AADownloadOutcome_Type> StartDownload()
        {
            await GetDownloadSize();
            if (_totalDownloadSize == 0)
                return AADownloadOutcome_Type.NoDownloadRequired;

            for (int i = 0; i < _preDownload_List.Count; i++)
            {
                //计算总下载进度
                float weightPerOperation = 1f / _preDownload_List.Count;

                AsyncOperationHandle asyncOperationHandle =
                    Addressables.DownloadDependenciesAsync(_preDownload_List[i]);
                while (!asyncOperationHandle.IsDone)
                {
                    //计算总进度
                    float progress = asyncOperationHandle.PercentComplete;
                    DownloadTheTotalProgress = progress * weightPerOperation;
                    //休息一会
                    await UniTask.DelayFrame(10);
                }

                if (asyncOperationHandle.Status == AsyncOperationStatus.Failed)
                {
                    AADownloadOutcome_Type ret;
                    bool netState = await NetworkTester.TestNetworkAsync();
                    if (!netState)
                        ret = AADownloadOutcome_Type.NetworkAbnormal;
                    else
                        ret = AADownloadOutcome_Type.Mistake;
                    Log.Error($"AddressablePreDownload 预下载{_preDownload_List[i]}失败 \n当前网络状态:{netState}");
                    return ret;
                }
            }

            DownloadTheTotalProgress = 1;
            return AADownloadOutcome_Type.DownloadIsComplete;
        }
    }
}