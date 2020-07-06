using EventArgs;
using Level.Packages;
using Score;
using TMPro;
using UnityEngine;

namespace Level.Containers
{
    public class ColorContainer : Container
    {
        public Color Color;

        protected override void OnPackageReleased(object sender, PackageEventArgs e)
        {
            if (!(e.Package is ColorPackage package))
            {
                Debug.LogWarning($"Got unrecognized package in color container: {e.Package}");
                return;
            }

            if (Color.Compare(package.Color))
            {
                ScoreManager.AddScore(package.Score);
            }

            package.Destroy();
        }
    }
}
