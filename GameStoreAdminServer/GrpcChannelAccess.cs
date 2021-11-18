
using Grpc.Net.Client;

namespace GameStoreAdminServer
{
    public sealed class GrpcChannelAccess {  
        private GrpcChannelAccess() {}  
        private static GrpcChannel _instance;  
        public static GrpcChannel Instance {  
            get => _instance;
            set => _instance = value;
        }  
    } 
}