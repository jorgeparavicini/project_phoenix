using Level.Packages;

namespace EventArgs
{
    public class PackageEventArgs
    {
        public Package Package { get; }

        public PackageEventArgs(Package package)
        {
            Package = package;
        }
    }
}