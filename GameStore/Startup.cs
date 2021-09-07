using System;
using System.Collections.Generic;
using System.Text;
using Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ServiceFactory fabricaDeServicios = new ServiceFactory(services);
            fabricaDeServicios.AgregarServicios();
            fabricaDeServicios.AgregarServicioContextoBD(this.Configuration.GetConnectionString("GameStoreDB"));
        }
    }
}
