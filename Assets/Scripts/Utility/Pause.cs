using UnityEditor;
using UnityEngine;

namespace Utility
{
    public class Pause : MonoBehaviour
    {
        public KeyCode KeyCode;
        
        #if UNITY_EDITOR

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                EditorApplication.isPaused = true;
            }
        }

#endif
    }
}
