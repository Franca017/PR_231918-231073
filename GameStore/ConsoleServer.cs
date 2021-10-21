using System;

namespace GameStoreServer
{
    class ConsoleServer
    {
        private static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            var setup = new Setup();
            _serviceProvider = setup.BuildServiceProvider();

            setup.InitializeSocketServer(_serviceProvider);

        }
    }
}
