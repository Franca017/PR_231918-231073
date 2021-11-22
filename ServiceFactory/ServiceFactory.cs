using Logic;
using LogicInterface;
using Microsoft.Extensions.DependencyInjection;
using ProtocolLibrary.FileHandler;
using ProtocolLibrary.FileHandler.Interfaces;
using Repository;
using RepositoryInterface;

namespace Factory
{
    public class ServiceFactory
    {
        private readonly IServiceCollection _services;

        public ServiceFactory(IServiceCollection services)
        {
            this._services = services;
        }

        public void ConfigureServices()
        {
            _services.AddSingleton<IGamesLogic, GamesLogic>();
            _services.AddSingleton<IUserLogic, UserLogic>();
            _services.AddSingleton<IReviewLogic, ReviewLogic>();
            _services.AddScoped<ILogBuilderLogic, LogBuilderLogic>();

            _services.AddSingleton<IGameRepository, GameRepository>();
            _services.AddSingleton<IUserRepository, UserRepository>();
            _services.AddSingleton<IReviewRepository, ReviewRepository>();

            _services.AddSingleton<IFileHandler, FileHandler>();
            _services.AddSingleton<IFileStreamHandler, FileStreamHandler>();
        }
    }
}
