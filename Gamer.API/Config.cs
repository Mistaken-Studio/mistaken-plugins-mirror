using Exiled.API.Interfaces;

namespace Gamer.API
{
    /// <summary>
    /// Basic Config
    /// </summary>
    public class Config : IConfig
    {
        /// <summary>
        /// If plugin should be enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
