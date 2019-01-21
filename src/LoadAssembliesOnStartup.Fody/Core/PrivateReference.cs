namespace LoadAssembliesOnStartup.Fody
{
    public class PrivateReference
    {
        public PrivateReference(string packageName, string version)
        {
            PackageName = packageName;
            Version = version;
        }

        public string PackageName { get; }

        public string Version { get; }

        public override string ToString()
        {
            return $"{PackageName} v{Version}";
        }
    }
}
