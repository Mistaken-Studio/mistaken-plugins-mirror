namespace Gamer.Utilities
{
    public interface IPermissionLocked
    {
        string Permission { get; }

        string PluginName { get; }
    }
}
