using System;
using Logic;
using LogicInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary.FileHandler;
using ProtocolLibrary.FileHandler.Interfaces;
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

        public void ConfigureServices()
        {
            services.AddSingleton<IGamesLogic, GamesLogic>();
            services.AddSingleton<IUserLogic, UserLogic>();
            services.AddSingleton<IReviewLogic, ReviewLogic>();

            services.AddSingleton<IGameRepository, GameRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IReviewRepository, ReviewRepository>();

            services.AddSingleton<IFileHandler, FileHandler>();
            services.AddSingleton<IFileStreamHandler, FileStreamHandler>();
        }
    }
}
