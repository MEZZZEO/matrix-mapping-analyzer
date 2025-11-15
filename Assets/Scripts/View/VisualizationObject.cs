using Infrastructure;
using UnityEngine;

namespace View
{
    [RequireComponent(typeof(Renderer))]
    public class VisualizationObject : MonoBehaviour, IPoolable
    {
        private Renderer _cachedRenderer;
        private MaterialPropertyBlock _propertyBlock;
        
        private static readonly int ColorPropertyID = Shader.PropertyToID("_Color");
        
        private void Awake()
        {
            _cachedRenderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
        }
        
        public void SetColor(Color color)
        {
            _propertyBlock.SetColor(ColorPropertyID, color);
            _cachedRenderer.SetPropertyBlock(_propertyBlock);
        }

        #region IPoolable Implementation
        
        public void OnSpawn()
        {
            
        }
        
        public void OnDespawn()
        {
            if (_propertyBlock != null && _cachedRenderer != null)
            {
                _propertyBlock.Clear();
                _cachedRenderer.SetPropertyBlock(_propertyBlock);
            }
        }

        #endregion
    }
}