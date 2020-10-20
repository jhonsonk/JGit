using System.IO;

namespace Tools
{
    public static class FileTools
    {
        /// <summary>
        /// Save the file
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="byteFile">File in byte array format</param>
        /// <param name="directoryPath">Destination directory path</param>
        /// <returns>Return the file path</returns>
        public static string SaveFile(string fileName, byte[] byteFile, string directoryPath = "")
        {
            string filePath = Path.Combine(directoryPath,fileName);

            CreateDirectory(directoryPath);

            using (FileStream file = new FileStream(filePath, FileMode.Create))
            {
                file.Write(byteFile, 0, byteFile.Length);
            }

            return filePath;
        }

        /// <summary>
        /// Create a directory if it does not exist
        /// </summary>
        /// <param name="path">Directory path</param>
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}