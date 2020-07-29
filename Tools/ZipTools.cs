using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Tools
{
    public static class ZipTools
    {
        /// <summary>
        /// Extract a zip file to a destination directory
        /// </summary>
        /// <param name="zipPath">Zip path</param>
        /// <param name="directoryPath">Destination directory Path</param>
        public static void ExtractZip(string zipPath, string directoryPath)
        {
            ZipFile.ExtractToDirectory(zipPath, directoryPath, Encoding.UTF8);
        }

        /// <summary>
        /// Gets the archives present in a zip
        /// </summary>
        /// <param name="zipPath"></param>
        /// <returns></returns>
        public static List<ZipArchiveEntry> GetZipArchives(string zipPath)
        {
            using (ZipArchive zipArchive = ZipFile.OpenRead(zipPath))
            {
                return zipArchive.Entries.ToList();
            }
        }

        /// <summary>
        /// Groups archives by their extension
        /// </summary>
        /// <param name="listArchives">List of Archives</param>
        /// <returns>returns a list of extension groups with a list of ZipArchiveEntry path</returns>
        public static List<IGrouping<string, ZipArchiveEntry>> GroupByExtension(List<ZipArchiveEntry> listArchives)
        {
            //remove all empty folders
            listArchives.RemoveAll(a => String.IsNullOrEmpty(a.Name));
            return listArchives.GroupBy(a => Path.GetExtension(a.Name)).ToList();
        }
    }
}