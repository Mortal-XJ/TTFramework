using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace GameFrame
{
    public enum ResourceType
    {
        ADDRESSABLES, //可寻址资源
        RESOURCE, //Resource文件夹资源
    }

    class ResourceUnit<T> where T : UnityEngine.Object
    {
        private string _path;
        private T _data;
        private ResourceType _resourceType;
        public T Data => _data;


        public ResourceUnit(string _path, T _data, ResourceType resourceType)
        {
            this._data = _data;
            this._path = _path;
            this._resourceType = resourceType;
        }

        /// <summary>
        /// 该路径是否等于这个资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsEqual(string path, ResourceType resourceType)
        {
            return path == this._path && resourceType == _resourceType;
        }

        /// <summary>
        /// 是否是该资源
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public bool IsEqual(T res)
        {
            return res == this._data;
        }

        public bool UnloadAsset()
        {
            switch (_resourceType)
            {
                case ResourceType.RESOURCE:
                    Resources.UnloadAsset(_data);
                    break;
                case ResourceType.ADDRESSABLES:
                    Addressables.Release(_data);
                    break;
            }
            if (_data == null)
                return true;
            return false;
        }
        
    }


    public class ExplorerHandler : Singleton<ExplorerHandler>
    {
        private ArrayList _cache = new ArrayList();


        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="resourceType">获取资源的方式</param>
        /// <typeparam name="T">获取的类型</typeparam>
        /// <returns></returns>
        public async UniTask<T> LoadResourceAsync<T>(string path, ResourceType resourceType = ResourceType.ADDRESSABLES)
            where T : UnityEngine.Object
        {
            if (TryContains<T>(path, out T res, resourceType))
                return res;
            switch (resourceType)
            {
                case ResourceType.ADDRESSABLES:
                    res = await Addressables.LoadAssetAsync<T>(path).ToUniTask();
                    break;
                case ResourceType.RESOURCE:
                    res = await Resources.LoadAsync<T>(path).ToUniTask() as T;
                    break;
            }

            if (res == null)
                throw new Exception($"ExplorerHandler 资源加载异常无法获取 {resourceType} : {path}");
            ResourceUnit<T> explorerUnit = new ResourceUnit<T>(path, res, resourceType);
            _cache.Add(explorerUnit);
            return res;
        }


        /// <summary>
        /// 是否存在该资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool TryContains<T>(string path, out T res, ResourceType resourceType = ResourceType.ADDRESSABLES)
            where T : UnityEngine.Object
        {
            for (int i = 0; i < _cache.Count; i++)
            {
                ResourceUnit<T> explorerUnit = (_cache[i] as ResourceUnit<T>);
                if (explorerUnit.IsEqual(path, resourceType))
                {
                    res = explorerUnit.Data;
                    return true;
                }
            }

            res = default;
            return false;
        }

        public bool UnloadAsset<T>(T res) where T : UnityEngine.Object
        {
            if (res == null) return false;
            for (int i = 0; i < _cache.Count; i++)
            {
                ResourceUnit<T> resourceUnit = _cache[i] as ResourceUnit<T>;
                if (resourceUnit.IsEqual(res))
                {
                    bool result = resourceUnit.UnloadAsset();
                    if (result)
                        _cache.Remove(resourceUnit);
                    return result;
                }
            }

            return false;
        }

        public bool UnloadAsset<T>(string res, ResourceType resourceType) where T : UnityEngine.Object
        {
            for (int i = 0; i < _cache.Count; i++)
            {
                ResourceUnit<T> resourceUnit = _cache[i] as ResourceUnit<T>;
                if (resourceUnit.IsEqual(res, resourceType))
                {
                    bool result = resourceUnit.UnloadAsset();
                    if (result)
                        _cache.Remove(resourceUnit);
                    return result;
                }
            }

            return false;
        }

        public override void Finish()
        {
            for (int i = 0; i < _cache.Count; i++)
            {
                ResourceUnit<UnityEngine.Object> resourceUnit = _cache[i] as ResourceUnit<UnityEngine.Object>;
                bool result = resourceUnit.UnloadAsset();
            }

            _cache.Clear();
        }
    }
}