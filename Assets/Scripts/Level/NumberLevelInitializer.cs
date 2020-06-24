using System;
using System.Collections.Generic;
using Data;
using Extensions;
using Level.Containers;
using Level.Packages;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level
{
    public class NumberLevelInitializer : MonoBehaviour
    {
        public List<NumberContainer> Containers = new List<NumberContainer>();
        public List<PackageSpawner> Spawners = new List<PackageSpawner>();
        public int RangeMax = 255;
        public int RangeMin = 1;
        public NumberBase PackageBase;
        public NumberBase ContainerBase;
        public NumberPackage _numberPackagePrefab;
        public int NumberOfDistinctPackages => Containers.Count;

        private readonly List<int> _usedNumbers = new List<int>();

        private void Start()
        {
            for (var i = 0; i < NumberOfDistinctPackages; i++)
            {
                var number = GetNextUniqueNumber();
                Containers[i].Value = number;
                Containers[i].Base = ContainerBase;
                
                Spawners.ForEach(p =>
                {
                    var spawnItem = new SpawnItem(_numberPackagePrefab.gameObject);
                    spawnItem.Spawned += (sender, args) =>
                    {
                        var package = (NumberPackage) args.Package;
                        package.Base = PackageBase;
                        package.Value = number;
                    };

                    p.SpawnItems.Add(spawnItem);
                });
            }
        }

        private int GetNextUniqueNumber()
        {
            while (true)
            {
                var num = Random.Range(RangeMin, RangeMax);
                if (_usedNumbers.Contains(num)) continue;

                _usedNumbers.Add(num);
                return num;
            }
        }
    }
}
