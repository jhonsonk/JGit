using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tools;

namespace Service
{

    public class ResponsePerExtension
    {
        public string Extension { get; set; }
        public long TotalLines { get; set; }
        public long TotalBytes { get; set; }
        public long TotalFiles { get; set; }
    }

    public class ResponsePerRepository
    {
        public string NameDirectory { get; set; }
        public long TotalLines { get; set; }
        public long TotalBytes { get; set; }
        public long TotalFiles { get; set; }
        public List<ResponsePerExtension> PerExtension { get; set; }
    }



    public class Class1
    {
        private string pathTemp = "";
        public Class1()
        {
            pathTemp = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\temp\" + Guid.NewGuid().ToString() + @"\";
        }

        private string DOWNLOAD_ERRO = "Ocorreu um erro no acesso ao reposirtorio";

        //Ainda tenho q pegar o URL e partir ela
        public ResponsePerRepository Clone(string gitUrl)
        {
            String nameFile = "master.zip";

            Uri uri = new Uri(gitUrl);

            if (uri.Segments.Length > 3)
                nameFile = uri.Segments[4].Split('.')[0] + ".zip";


            string urlBase = uri.Scheme + "://" + uri.Host + uri.Segments[0] + uri.Segments[1] + uri.Segments[2] + "archive/" + nameFile;


            //variabele of return
            List<ResponsePerExtension> returnFileExtensions = new List<ResponsePerExtension>();

            byte[] byteFile;
            //Downdload archive
            try { byteFile = Task.Run(() => DownloadTools.DownloadFile(urlBase)).Result; }
            catch { throw new Exception(DOWNLOAD_ERRO); }

            string filePath = "";
            //Save archive 
            try { filePath = FileTools.SaveFile(nameFile, byteFile, pathTemp); }
            catch { throw new Exception(DOWNLOAD_ERRO); }

            //Get list of files 
            List<ZipArchiveEntry> zipArchive;
            try { zipArchive = ZipTools.GetZipArchives(filePath); }
            catch { throw new Exception(DOWNLOAD_ERRO); }

            //Group by File extension
            List<IGrouping<string, ZipArchiveEntry>> groupExtensions = ZipTools.GroupByExtension(zipArchive);

            //Extract files
            ZipTools.ExtractZip(filePath, pathTemp);

            //Interaction on the file list
            foreach (IGrouping<string, ZipArchiveEntry> item in groupExtensions)
            {
                returnFileExtensions.Add(Count(item));
            }

            //Delete all temporary files
            FileTools.DeleteDirectory(pathTemp);

            //Repository totals
            return new ResponsePerRepository()
            {
                TotalBytes = returnFileExtensions.Select(a => a.TotalBytes).Sum(),
                TotalFiles = returnFileExtensions.Select(a => a.TotalFiles).Sum(),
                TotalLines = returnFileExtensions.Select(a => a.TotalLines).Sum(),
                PerExtension = returnFileExtensions
            };
        }

        public ResponsePerExtension Count(IGrouping<string, ZipArchiveEntry> archives)
        {
            //Objet of return
            ResponsePerExtension perExtension = new ResponsePerExtension();

            //Get the extension file
            perExtension.Extension = archives.Key;

            //Count of files with extension
            perExtension.TotalFiles = archives.ToList().Count();

            //Count of bytes of all files with extension
            perExtension.TotalBytes = archives.ToList().Select(a => a.Length).Sum();

            //interresao sobre a lista de arquivos
            foreach (ZipArchiveEntry archive in archives.ToList())
            {
                try
                {
                    int lines = File.ReadAllLines(pathTemp + archive.FullName).Count();
                    perExtension.TotalLines += lines;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return perExtension;
        }
    }
}
