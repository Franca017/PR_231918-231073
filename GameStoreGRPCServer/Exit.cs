namespace GameStoreGRPCServer
{
    public sealed class Exit {  
        private Exit() {}
        public static bool Instance { get; set; } = false;
    } 
}