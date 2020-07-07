using UnityEngine;

namespace Phoenix.Level.Packages
{
    public class PackageDestroyer : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Package.Tag)) return;
            other.GetComponent<Package>().Destroy();
        }
    }

}
