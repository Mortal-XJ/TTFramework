using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Internal;
using UnityEngine;

namespace GameFrame
{
    public class PoolHandler : Singleton<PoolHandler>
    {
        private ConcurrentDictionary<string, Pool> _pools = new ConcurrentDictionary<string, Pool>();
        private int _poolSize = 20; //每个池子的大小

        async UniTask<GameObject> Load(string path)
        {
            if (_pools.TryGetValue(path, out Pool pool))
                return  pool.Release();
            GameObject go = await ExplorerHandler.Instance.LoadResourceAsync<GameObject>(path);
            pool = new Pool(go, _poolSize);
            _pools.TryAdd(path, pool);
            return  pool.Release();
        }

        public async UniTask<GameObject> Release(string path, Vector3 position)
        {
            GameObject go = await this.Load(path);
            go.transform.position = position;
            return go;
        }

        public async UniTask<GameObject> Release(string path, Vector3 position, Quaternion rotation)
        {
            GameObject go = await this.Release(path, position);
            go.transform.rotation = rotation;
            return go;
        }

        public async UniTask<GameObject> Release(string path, Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            GameObject go = await this.Release(path, position, rotation);
            go.transform.localScale = localScale;
            return go;
        }

        public void Reclaim(GameObject go)
        {
            foreach (var pool in _pools)
            {
                if (pool.Value.GoName == go.name)
                {
                    pool.Value.Reclaim(go);
                    return;
                }
            }
        }
    }

    class Pool
    {
        private ConcurrentQueue<GameObject> _pool = new ConcurrentQueue<GameObject>();
        public string GoName { get; private set; }

        public Pool(GameObject go, int quantity)
        {
            GoName = go.name;
            this.Load(go, quantity);
        }

        async UniTask Load(GameObject go, int q)
        {
            GameObject instan;
            for (int i = 0; i < q; i++)
            {
                instan = GameObject.Instantiate(go);
                this.InitTans(go);
                _pool.Enqueue(instan);
            }
        }

        public GameObject Release()
        {
            _pool.TryDequeue(out GameObject go);
            Error.ThrowArgumentNullException(go, go.name);
            return go;
        }

        public void Reclaim(GameObject go)
        {
            this.InitTans(go);
            _pool.Enqueue(go);
        }

        void InitTans(GameObject go)
        {
            go.transform.position = new Vector3(1000, 1000, 1000);
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
        }
    }
}