using UnityEngine;

namespace Level
{
    public class ConveyorBelt : MonoBehaviour
    {
        public Vector3 Speed;
        public Vector2 ShaderSpeed;
        public bool UseLegacyShader;

        private Vector2 _shaderOffset;
        private Renderer _materialRenderer;
        private Rigidbody _rb;
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        private void Start()
        {
            _materialRenderer = GetComponent<Renderer>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            _shaderOffset += Time.deltaTime * ShaderSpeed;
            if (UseLegacyShader)
            {
                _materialRenderer.material.mainTextureOffset = _shaderOffset;
            }
            else
            {
                _materialRenderer.material.SetTextureOffset(BaseMap, _shaderOffset);
            }
        }

        private void OnCollisionStay(Collision _)
        {
            var movement = Speed * Time.deltaTime;
            var position = _rb.position;
            position -= movement;
            _rb.position = position;
            _rb.MovePosition(position + movement);
        }
    }
}
