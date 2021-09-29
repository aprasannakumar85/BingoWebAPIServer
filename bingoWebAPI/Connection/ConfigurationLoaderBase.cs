using Microsoft.Extensions.Configuration;

namespace bingoWebAPI.Connection
{
    public abstract class ConfigurationLoaderBase
    {
        private readonly IConfiguration _config;
        public IConfiguration Config => _config;

        protected ConfigurationLoaderBase(IConfiguration config)
        {
            _config = config;
        }
    }
}
