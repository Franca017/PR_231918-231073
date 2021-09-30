namespace ProtocolLibrary
{
    public class HeaderConstants
    {
        public const string Request = "REQ";
        public const string Response = "RES";
        public const int CommandLength = 2;
        public const int DataLength = 4;
        public const int FixedFileNameLength = 4;
        public const int FixedFileSizeLength = 8;
        public const int MaxPacketSize = 32768;
    }
}