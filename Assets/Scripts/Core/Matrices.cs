using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Core
{
    public class Matrices : IReadOnlyList<Matrix>
    {
        private const int DefaultDigits = 5;

        private readonly List<Matrix> _items;
        private readonly HashSet<ulong> _hashes;
        
        public bool Contains(ulong hash)
        {
            return _hashes.Contains(hash);
        }

        public Matrix this[int index] => _items[index];
        public int Count => _items.Count;

        public IEnumerator<Matrix> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public Matrices(string path, int digits = DefaultDigits)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}");
            }

            var json = File.ReadAllText(path);
            var matrices = JsonConvert.DeserializeObject<Matrix4x4[]>(json);

            _items = matrices.Select(m => new Matrix(m, digits)).ToList();
            _hashes = new HashSet<ulong>(_items.Select(m => m.Hash));
        }
    }
}