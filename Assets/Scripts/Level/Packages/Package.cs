using System;
using System.Collections;
using EventArgs;
using Interfaces;
using UnityEngine;

namespace Level.Packages
{
    [Serializable]
    public abstract class Package : MonoBehaviour, IDestroyable
    {
        public const string Tag = "Package";
        public Rigidbody RigidBody { get; private set; }
        public bool IsPlayerControlled { get; private set; }

        public event EventHandler<PackageEventArgs> PackageReleased = delegate { };
        public event EventHandler<PackageEventArgs> PackagePickedUp = delegate { };

        private void Start()
        {
            RigidBody = GetComponent<Rigidbody>();
        }

        public void PlayerPickUp()
        {
            IsPlayerControlled = true;
            PackagePickedUp(this, new PackageEventArgs(this));
        }

        public void PlayerDrop()
        {
            IsPlayerControlled = false;
            PackageReleased(this, new PackageEventArgs(this));
        }

        public virtual void Destroy()
        {
            StartCoroutine(Destroy(1.5f));
        }

        private IEnumerator Destroy(float time)
        {
            var originalScale = transform.localScale;
            var destinationScale = Vector3.zero;

            var currentTime = 0f;

            do
            {
                transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / time);
                currentTime += Time.deltaTime;
                yield return null;
            } while (currentTime <= time);

            Destroy(gameObject);
        }
    }
}
