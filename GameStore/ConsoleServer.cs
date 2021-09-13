using System;

namespace GameStoreServer
{
    class ConsoleServer
    {
        private static IServiceProvider _serviceProvider;


        static void Main(string[] args)
        {
            Setup setup = new Setup();
            _serviceProvider = setup.BuildServiceProvider();
            Runtime runtime = new Runtime(_serviceProvider);

            setup.InitializeSocketServer(runtime);

        }
    }
}
