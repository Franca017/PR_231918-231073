
using System.Threading.Tasks;

namespace GameStoreClient
{
    class ConsoleClient
    {
        static async Task Main(string[] args)
        {
            var setup = new Setup();
            var runtime = new Runtime();

            var socket = await setup.InitializeSocketServer();
            
            await runtime.Execute(socket);
        }
    }
}
