
namespace GameStoreAdminServer
{
    public sealed class GrpcClient {  
        private GrpcClient() {}  
        private static Greeter.GreeterClient _instance;  
        public static Greeter.GreeterClient Instance {  
            get => _instance;
            set => _instance = value;
        }  
    } 
}