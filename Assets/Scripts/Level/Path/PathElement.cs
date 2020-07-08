using System.Collections;
using UnityEngine;

namespace Phoenix.Level.Path
{
    public class PathElement : MonoBehaviour
    {
        [SerializeField] private PathDirection _direction = PathDirection.None;
        private bool _valid;
        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int Color = Shader.PropertyToID("_BaseColor");
        private static readonly int TopProgress = Shader.PropertyToID("Vector1_A6CAEB63");
        
        public Material HoverMaterial;
        public Material FixedMaterial;
        public Color InvalidColor = UnityEngine.Color.red;
        public Color ValidColor = UnityEngine.Color.green;
        public bool Connected;
        
        public PathDirection Direction => _direction;
        public GridPoint GridPoint { get; set; }

        public bool Valid
        {
            get => _valid;
            set
            {
                var old = _valid;
                _valid = value;
                if (old != _valid) UpdateMaterial();
            }
        }

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
        }
        
        private void UpdateMaterial()
        {
            _propertyBlock.SetColor(Color, Valid ? ValidColor : InvalidColor);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void OnPlace()
        {
            
        }

        public IEnumerator Connect(PathDirection direction)
        {
            var count = 0f;
            while (count < 1)
            {
                count += Time.deltaTime;
                _propertyBlock.SetFloat(TopProgress, count);
                _renderer.SetPropertyBlock(_propertyBlock);
                yield return null;
            }
            _propertyBlock.SetColor(Color, UnityEngine.Color.blue);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
