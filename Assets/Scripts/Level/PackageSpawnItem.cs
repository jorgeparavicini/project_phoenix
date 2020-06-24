using System;
using EventArgs;
using UnityEngine;

namespace Level
{
    [Serializable]
    public class SpawnItem
    {
        public bool HasMaxSpawns;
        public int MaxSpawns;
        public GameObject Package;
        public int Priority;
        public int TimesSpawned { get; private set; }

        public bool CanSpawn => !HasMaxSpawns || TimesSpawned < MaxSpawns;
        public event EventHandler<PackageEventArgs> Spawned = delegate { };


        public SpawnItem()
        {
        }

        public SpawnItem(GameObject package, bool hasMaxSpawns = false, int maxSpawns = 0, int priority = 1)
        {
            Package = package;
            HasMaxSpawns = hasMaxSpawns;
            MaxSpawns = maxSpawns;
            Priority = priority;
        }


        public void OnSpawn(object sender, PackageEventArgs e)
        {
            TimesSpawned++;
            Spawned(sender, e);
        }
    }
}
