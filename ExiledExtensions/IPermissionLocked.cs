namespace Gamer.Utilities
{
    /// <summary>
    /// Interface used to mark that command requires permissions
    /// </summary>
    public interface IPermissionLocked
    {
        /// <summary>
        /// Permission
        /// </summary>
        string Permission { get; }
        /// <summary>
        /// Plugin name, used as prefix
        /// </summary>
        string PluginName { get; }
    }
}
