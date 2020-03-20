using KeePass.Plugins;

namespace OnePasswordImporterPlugin
{
    public sealed class OnePasswordImporterPluginExt : Plugin
    {
        private IPluginHost _host;
        private OnePasswordFileFormatProvider _provider;

        public override bool Initialize(IPluginHost host)
        {
            _host = host;
            _provider = new OnePasswordFileFormatProvider();

            _host.FileFormatPool.Add(_provider);

            return true;
        }

        public override void Terminate()
        {
            _host.FileFormatPool.Remove(_provider);
        }
    }
}
