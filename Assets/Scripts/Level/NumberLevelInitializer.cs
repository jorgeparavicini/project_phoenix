using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<NumberContainer> _containers = new List<NumberContainer>();
        private List<PackageSpawner> _spawners = new List<PackageSpawner>();
        public int RangeMax = 255;
        public int RangeMin = 1;
        public NumberBase PackageBase;
        public NumberBase ContainerBase;
        public NumberPackage _numberPackagePrefab;
        public int NumberOfDistinctPackages => _containers.Count;

        private readonly List<int> _usedNumbers = new List<int>();

        private void Start()
        {
            _containers = GameObject.FindGameObjectsWithTag(Container.Tag)
                .Select((obj, index) => GetComponent<NumberContainer>())
                .ToList();
            _spawners = GameObject.FindGameObjectsWithTag(PackageSpawner.Tag)
                .Select((obj, index) => GetComponent<PackageSpawner>())
                .ToList();

            for (var i = 0; i < NumberOfDistinctPackages; i++)
            {
                var number = GetNextUniqueNumber();
                _containers[i].Value = number;
                _containers[i].Base = ContainerBase;

                _spawners.ForEach(p =>
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