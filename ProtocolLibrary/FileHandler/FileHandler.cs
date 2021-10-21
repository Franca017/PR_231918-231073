using System;
using System.IO;
using ProtocolLibrary.FileHandler.Interfaces;

namespace ProtocolLibrary.FileHandler
{
    public class FileHandler : IFileHandler
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string GetFileName(string path)
        {
            if (FileExists(path))
            {
                return new FileInfo(path).Name;
            }

            throw new Exception("File does not exist");
        }

        public long GetFileSize(string path)
        {
            if (FileExists(path))
            {
                return new FileInfo(path).Length;
            }

            throw new Exception("File does not exist");
        }
    }
}