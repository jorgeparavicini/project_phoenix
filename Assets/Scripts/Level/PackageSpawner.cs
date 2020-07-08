using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Phoenix.EventArgs;
using Phoenix.Level.Packages;
using UnityEngine;
using Random = System.Random;

namespace Phoenix.Level
{
    public abstract class PackageSpawner : MonoBehaviour
    {
        public const string Tag = "PackageSpawner";


        private IEnumerator _spawnerCoroutine;

        public Transform PackageParent;
        public List<SpawnItem> SpawnItems = new List<SpawnItem>();


        public bool Spawning { get; private set; }


        public List<SpawnItem> SpawnablePackages => SpawnItems.Where(x => x.CanSpawn).ToList();
        public int TotalPriority => SpawnablePackages.Sum(x => x.Priority);
        public event EventHandler<PackageEventArgs> PackageSpawned = delegate { };


        private SpawnItem GetPackage()
        {
            var random = new Random().NextDouble();
            var priorityCounter = 0f;
            foreach (var package in SpawnablePackages)
            {
                var nextPriority = priorityCounter + (float) package.Priority / TotalPriority;
                if (priorityCounter < random && random < nextPriority) return package;

                priorityCounter = nextPriority;
            }

            throw new InvalidOperationException("Failed to get a random priority based package");
        }

        private void Spawn()
        {
            if (SpawnablePackages.Count == 0)
            {
                Spawning = false;
                return;
            }

            if (!Spawning) return;

            var packageObj = GetPackage();
            var localTransform = transform;
            var position = localTransform.position;
            var instance = Instantiate(packageObj.Package, position, localTransform.rotation, PackageParent);
            var package = instance.GetComponent<Package>();
            packageObj.OnSpawn(this, new PackageEventArgs(package));
            PackageSpawned(this, new PackageEventArgs(package));
        }

        private IEnumerator SpawnerRoutine()
        {
            Spawning = true;
            while (Spawning)
            {
                yield return StartCoroutine(SpawnDelay());

                Spawn();
            }
        }

        protected abstract IEnumerator SpawnDelay();

        public void Stop()
        {
            Spawning = false;

            if (_spawnerCoroutine is null) return;

            StopCoroutine(_spawnerCoroutine);
            _spawnerCoroutine = null;
        }

        public void StartSpawner()
        {
            if (_spawnerCoroutine != null)
            {
                Debug.LogWarning("Already running a spawner coroutine");
                return;
            }

            _spawnerCoroutine = SpawnerRoutine();
            StartCoroutine(_spawnerCoroutine);
        }
    }
}
