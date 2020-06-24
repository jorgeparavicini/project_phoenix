using System;
using EventArgs;
using Level.Packages;
using UnityEngine;

namespace Level.Containers
{
    /// <summary>
    /// An abstract class for all containers in the project.
    /// It creates overridable methods for handling package events.
    /// </summary>
    public abstract class Container : MonoBehaviour
    {
        /// <summary>
        /// Handles the entering of a package inside a container.
        /// It makes sure only packages are handled and not arbitrary objects.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(Package.Tag)) return;

            var package = other.gameObject.GetComponent<Package>();

            // Thrown only if the scene is in an invalid state and *needs* to be fixed.
            if (package is null)
                throw new InvalidOperationException($"Package without package script: {other.gameObject}");

            OnPackageEnter(this, new PackageEventArgs(package));

            // If the package is still controlled by the player (it is still being hold in the hand)
            // then we listen for the release event on the package.
            if (package.IsPlayerControlled)
                package.PackageReleased += OnPackageReleased;
            else
            {
                OnPackageReleased(this, new PackageEventArgs(package));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.CompareTag(Package.Tag)) return;

            var package = other.gameObject.GetComponent<Package>();

            // Thrown only if the scene is in an invalid state and *needs* to be fixed.
            if (package is null)
                throw new InvalidOperationException($"Package without package script: {other.gameObject}");

            OnPackageExit(this, new PackageEventArgs(package));
            package.PackageReleased -= OnPackageReleased;
        }

        /// <summary>
        /// Called when a package has been released inside the container.
        /// It is guaranteed that the player is NOT controlling the package at this point.
        /// </summary>
        /// <param name="sender">The invoker of the event</param>
        /// <param name="e">The Event Args with the Package information</param>
        protected virtual void OnPackageReleased(object sender, PackageEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever a package entered the container.
        /// At this point the package could still be player controlled and should not be used to handle the release logic.
        /// </summary>
        /// <param name="sender">The invoker of the event</param>
        /// <param name="e">The Event Args with the Package information</param>
        protected virtual void OnPackageEnter(object sender, PackageEventArgs e)
        {
        }

        /// <summary>
        /// Called whenever a package exited the container.
        /// The player can hold a package, put it inside a container and remove it before releasing it,
        /// this method is meant to handle that situation.
        /// </summary>
        /// <remarks>
        /// The <see cref="OnPackageEnter"/> and <see cref="OnPackageExit"/> methods could be used as a hover effect
        /// where the container could show where the package would be placed once released but does not in fact release it.
        /// Only when <see cref="OnPackageReleased"/> is called the package should be treated as released inside the container.
        /// </remarks>
        /// <param name="sender">The invoker of the event</param>
        /// <param name="e">The Event Args with the Package information</param>
        protected virtual void OnPackageExit(object sender, PackageEventArgs e)
        {
        }
    }
}
