using System.Collections;
using System.Collections.Generic;
using EventArgs;
using Level.Packages;
using UnityEngine;

namespace Level.Packages
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
