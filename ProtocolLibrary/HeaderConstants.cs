namespace ProtocolLibrary
{
    public class HeaderConstants
    {
        public static string Request = "REQ";
        public static string Response = "RES";
        public static int CommandLength = 2;
        public static int DataLength = 4;
        public const int FixedFileNameLength = 4;
        public const int FixedFileSizeLength = 8;
        public const int MaxPacketSize = 32768;
    }
}