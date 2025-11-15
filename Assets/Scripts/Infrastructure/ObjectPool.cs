using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// Generic пул объектов с поддержкой любых компонентов, реализующих IPoolable.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который должен реализовывать IPoolable</typeparam>
    public class ObjectPool<T> where T : Component, IPoolable
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly List<T> _pool;
        private readonly int _initialSize;
        private readonly bool _autoExpand;

        /// <summary>
        /// Количество объектов в пуле
        /// </summary>
        public int PoolSize => _pool.Count;

        /// <summary>
        /// Количество активных (используемых) объектов
        /// </summary>
        public int ActiveCount
        {
            get
            {
                int count = 0;
                foreach (var item in _pool)
                {
                    if (item != null && item.gameObject.activeSelf)
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Количество свободных объектов в пуле
        /// </summary>
        public int AvailableCount => PoolSize - ActiveCount;

        /// <summary>
        /// Создает новый пул объектов
        /// </summary>
        /// <param name="prefab">Префаб, который должен содержать компонент типа T</param>
        /// <param name="parent">Родительский Transform для размещения объектов</param>
        /// <param name="initialSize">Начальный размер пула</param>
        /// <param name="autoExpand">Автоматически расширять пул при нехватке объектов</param>
        public ObjectPool(GameObject prefab, Transform parent, int initialSize = 10, bool autoExpand = true)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (prefab.GetComponent<T>() == null)
                throw new ArgumentException($"Prefab must have component of type {typeof(T).Name}");

            _prefab = prefab;
            _parent = parent;
            _initialSize = initialSize;
            _autoExpand = autoExpand;
            _pool = new List<T>(initialSize);

            // Предварительное создание объектов
            Prewarm();
        }

        /// <summary>
        /// Предварительное создание объектов в пуле
        /// </summary>
        public void Prewarm()
        {
            for (int i = _pool.Count; i < _initialSize; i++)
            {
                CreateNewObject();
            }
        }

        /// <summary>
        /// Получить объект из пула
        /// </summary>
        /// <returns>Компонент типа T из пула</returns>
        public T Get()
        {
            // Ищем свободный объект
            foreach (var item in _pool)
            {
                if (item != null && !item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(true);
                    item.OnSpawn();
                    return item;
                }
            }

            // Если свободных объектов нет и включено автоматическое расширение
            if (_autoExpand)
            {
                var newItem = CreateNewObject();
                newItem.gameObject.SetActive(true);
                newItem.OnSpawn();
                return newItem;
            }

            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Pool is empty and auto-expand is disabled!");
            return null;
        }

        /// <summary>
        /// Получить объект из пула с настройкой позиции и поворота
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            var item = Get();
            if (item != null)
            {
                item.transform.SetPositionAndRotation(position, rotation);
            }
            return item;
        }

        /// <summary>
        /// Вернуть объект в пул
        /// </summary>
        public void Return(T item)
        {
            if (item == null)
                return;

            if (!_pool.Contains(item))
            {
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Trying to return object that doesn't belong to this pool!");
                return;
            }

            item.OnDespawn();
            item.gameObject.SetActive(false);
        }

        /// <summary>
        /// Вернуть все активные объекты в пул
        /// </summary>
        public void ReturnAll()
        {
            foreach (var item in _pool)
            {
                if (item != null && item.gameObject.activeSelf)
                {
                    Return(item);
                }
            }
        }

        /// <summary>
        /// Очистить пул (уничтожить все объекты)
        /// </summary>
        public void Clear()
        {
            foreach (var item in _pool)
            {
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
            _pool.Clear();
        }

        /// <summary>
        /// Создает новый объект и добавляет в пул
        /// </summary>
        private T CreateNewObject()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            instance.SetActive(false);

            var component = instance.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[ObjectPool<{typeof(T).Name}>] Created object doesn't have required component!");
                UnityEngine.Object.Destroy(instance);
                return null;
            }

            _pool.Add(component);
            return component;
        }

        /// <summary>
        /// Получить все объекты в пуле (для отладки)
        /// </summary>
        public IReadOnlyList<T> GetAll() => _pool.AsReadOnly();
    }
}