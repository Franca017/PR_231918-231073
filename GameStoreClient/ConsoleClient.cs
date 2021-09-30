
namespace GameStoreClient
{
    class ConsoleClient
    {
        static void Main(string[] args)
        {
            var setup = new Setup();
            var runtime = new Runtime();

            var socket = setup.InitializeSocketServer();
            
            runtime.Execute(socket);
        }
    }
}
