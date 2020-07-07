using UnityEngine;
using UnityEngine.UI;
using Valve.VR.Extras;

namespace Phoenix.VR
{
    [RequireComponent(typeof(SteamVR_LaserPointer))]
    public class LaserPointerVrButtonHandler : MonoBehaviour
    {
        private const string ButtonTag = "VRButton";

        private SteamVR_LaserPointer _laserPointer;

        private void Awake()
        {
            _laserPointer = GetComponent<SteamVR_LaserPointer>();
            _laserPointer.PointerIn += LaserPointerOnPointerIn;
            _laserPointer.PointerClick += LaserPointerOnPointerClick;
        }

        private void LaserPointerOnPointerIn(object sender, PointerEventArgs e)
        {
            if (e.target.CompareTag(ButtonTag))
            {
                // TODO: Add hover effect
            }
        }

        private static void LaserPointerOnPointerClick(object sender, PointerEventArgs e)
        {
            if (e.target.CompareTag(ButtonTag))
            {
                e.target.GetComponent<Button>().onClick.Invoke();
            }
        }

        public void DisablePointer()
        {
            if (_laserPointer.pointer != null)
                _laserPointer.pointer.SetActive(false);
        }

        public void EnablePointer()
        {
            if (_laserPointer.pointer != null)
                _laserPointer.pointer.SetActive(true);
        }
    }
}
