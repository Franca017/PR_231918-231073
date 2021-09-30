using System;
using System.Text;

namespace ProtocolLibrary
{
    public class Header
    {
        private byte[] _direction;
        private byte[] _command;
        private byte[] _dataLength;

        private String _sDirection;
        private int _iCommand;
        private int _iDataLength;

        public string SDirection
        {
            get => _sDirection;
            set => _sDirection = value;
        }

        public int ICommand
        {
            get => _iCommand;
            set => _iCommand = value;
        }

        public int IDataLength
        {
            get => _iDataLength;
            set => _iDataLength = value;
        }

        public Header()
        {
        }

        public Header(string direction, int command, int datalength)
        {

            _direction = Encoding.UTF8.GetBytes(direction);
            var stringCommand =
                command.ToString("D2"); //Maximo largo 2, si es menor a 2 cifras, completo con 0s a la izquierda 
            _command = Encoding.UTF8.GetBytes(stringCommand);
            var stringData = datalength.ToString("D4"); // 0 < Largo <= 9999 
            _dataLength = Encoding.UTF8.GetBytes(stringData);
        }

        public byte[] GetRequest()
        {
            var header = new byte[HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                  HeaderConstants.DataLength];
            Array.Copy(_direction, 0, header, 0, 3);
            Array.Copy(_command, 0, header, HeaderConstants.Request.Length, 2);
            Array.Copy(_dataLength, 0, header, HeaderConstants.Request.Length + HeaderConstants.CommandLength, 4);
            return header;
        }

        public bool DecodeData(byte[] data)
        {
            _sDirection = Encoding.UTF8.GetString(data, 0, HeaderConstants.Request.Length);
            var command = Encoding.UTF8.GetString(data, HeaderConstants.Request.Length, HeaderConstants.CommandLength);
            _iCommand = int.Parse(command);
            var dataLength = Encoding.UTF8.GetString(data,
                HeaderConstants.Request.Length + HeaderConstants.CommandLength, HeaderConstants.DataLength);
            _iDataLength = int.Parse(dataLength);
            return true;
        }
        
        public static int GetLength()
        {
            return HeaderConstants.FixedFileNameLength + HeaderConstants.FixedFileSizeLength;
        }

        public byte[] Create(string fileName, long fileSize)
        {
            var header =
                new byte[GetLength()]; // Creo un array de bytes de largo Specification.FixedFileNameLength + Specification.FixedFileSizeLength;
            var fileNameData =
                BitConverter.GetBytes(Encoding.UTF8.GetBytes(fileName).Length); // Obtengo largo del nombre
            if (fileNameData.Length != HeaderConstants.FixedFileNameLength)
                throw new Exception("There is something wrong with the file name");
            var fileSizeData = BitConverter.GetBytes(fileSize); // Obtengo tamaño del archivo en array de bytes

            Array.Copy(fileNameData, 0,
                header, 0, HeaderConstants.FixedFileNameLength); // Copio al array destino XXXX a partir de la posicion 0
            Array.Copy(fileSizeData, 0, header,
                HeaderConstants.FixedFileNameLength, HeaderConstants.FixedFileSizeLength); // Copio al array de destino YYYYYYYY a partir de la posicion 4

            return header;
        }
        
        public static long GetParts(long fileSize)
        {
            var parts = fileSize / HeaderConstants.MaxPacketSize;
            return parts * HeaderConstants.MaxPacketSize == fileSize ? parts : parts + 1;
        }
    }
}