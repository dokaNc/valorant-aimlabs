using System.Collections.Generic;
using UnityEngine;

namespace ValorantAimTrainer.Core
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool;
        private readonly List<T> _activeObjects;
        private readonly int _initialSize;

        public int ActiveCount => _activeObjects.Count;
        public int PooledCount => _pool.Count;

        public ObjectPool(T prefab, Transform parent, int initialSize)
        {
            _prefab = prefab;
            _parent = parent;
            _initialSize = initialSize;
            _pool = new Queue<T>(initialSize);
            _activeObjects = new List<T>(initialSize);

            Prewarm();
        }

        private void Prewarm()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                T obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        private T CreateNewObject()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            return obj;
        }

        public T Get()
        {
            T obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            obj.gameObject.SetActive(true);
            _activeObjects.Add(obj);

            if (obj is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null) return;

            if (obj is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            obj.gameObject.SetActive(false);
            _activeObjects.Remove(obj);
            _pool.Enqueue(obj);
        }

        public void ReturnAll()
        {
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                Return(_activeObjects[i]);
            }
        }

        public void Clear()
        {
            ReturnAll();

            while (_pool.Count > 0)
            {
                T obj = _pool.Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj.gameObject);
                }
            }

            _activeObjects.Clear();
        }
    }

    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}
