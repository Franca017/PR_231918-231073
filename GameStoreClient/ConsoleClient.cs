
using System;
using System.Threading.Tasks;
using Domain.Exceptions;

namespace GameStoreClient
{
    class ConsoleClient
    {
        static async Task Main(string[] args)
        {
            var setup = new Setup();
            var runtime = new Runtime();
            try
            {
                var socket = await setup.InitializeSocketServerAsync();

                await runtime.ExecuteAsync(socket);
            }
            catch (ServerDisconnected s)
            {
                Console.WriteLine(s.Message);
            }
        }
    }
}
