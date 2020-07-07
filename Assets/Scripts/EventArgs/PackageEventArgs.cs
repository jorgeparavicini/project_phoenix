using Phoenix.Level.Packages;

namespace Phoenix.EventArgs
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