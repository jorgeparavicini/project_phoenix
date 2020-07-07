using System.Collections;
using UnityEngine;

namespace Phoenix.Level
{
    public class RandomTimePackageSpawner: PackageSpawner
    {
        public Vector2 Range;

        protected override IEnumerator SpawnDelay()
        {
            var random = Random.Range(Range.x, Range.y);
            yield return new WaitForSeconds(random);
        }
    }
}