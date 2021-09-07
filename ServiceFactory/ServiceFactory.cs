using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using RepositoryInterface;

namespace Factory
{
    public class ServiceFactory
    {
        private readonly IServiceCollection services;

        public ServiceFactory(IServiceCollection services)
        {
            this.services = services;
        }

        public void AgregarServicios()
        {
            /*services.AddScoped<ISesionLogica, SesionLogica>();
            services.AddScoped<IAdministradorLogica, AdministradorLogica>();
            services.AddScoped<ICategoriaLogica, CategoriaLogica>();*/

            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            
        }

        public void AgregarServicioContextoBD(string connectionString)
        {
            this.services.AddDbContext<DbContext, Context>(options => options.UseSqlServer(connectionString));
        }
    }
}
