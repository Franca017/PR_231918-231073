using Factory;
using Microsoft.Extensions.DependencyInjection;

namespace GameStoreServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ServiceFactory serviceFactory = new ServiceFactory(services);
            serviceFactory.ConfigureServices();
        }
    }
}
