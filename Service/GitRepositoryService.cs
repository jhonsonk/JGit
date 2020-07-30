using Service.Model;
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

    public class GitRepositoryService
    {
        #region Mensagens
        private string DOWNLOAD_ERROR = "Ocorreu um erro no acesso ao reposirtorio";
        private string SAVE_FILE_ERROR = "Ocorreu um erro no acesso ao reposirtorio";
        private string FILE_ERROR = "Ocorreu um erro no acesso ao reposirtorio";
        private string EXTRACT_ERROR = "Ocorreu um erro no acesso ao reposirtorio";
        #endregion


        private string tempPath = "";
        public GitRepositoryService()
        {
            //tempPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\temp\" + Guid.NewGuid().ToString() + @"\";
            tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "temp", Guid.NewGuid().ToString());
        }

        public ResponsePerRepository GetTotal(string gitUrl)
        {
            //variable of return
            List<ResponsePerExtension> returnFileExtensions = new List<ResponsePerExtension>();

            //Default name
            String nameFile = "master.zip";

            gitUrl = gitUrl.ToLower().Replace("http://", String.Empty).Replace("https://", String.Empty);

            var uri = gitUrl.Split('/');
            //is it a branch?
            if (uri.Length > 3)
                nameFile = uri[4].Split('.')[0] + ".zip";

            //Get Repository name
            string nameRepository = uri[2].Replace("/", String.Empty);

            //make url from download           
            string urlBase = "https://" + uri[0] + "/" + uri[1] + "/" + uri[2] + "/archive/" + nameFile;

            byte[] byteFile;
            //Downdload archive
            try { byteFile = Task.Run(() => DownloadTools.DownloadFile(urlBase)).Result; }
            catch { throw new Exception(DOWNLOAD_ERROR); }

            string filePath = "";
            //Save archive 
            try { filePath = FileTools.SaveFile(nameFile, byteFile, tempPath); }
            catch { throw new Exception(SAVE_FILE_ERROR); }

            //Get list of files 
            List<ZipArchiveEntry> zipArchive;
            try { zipArchive = ZipTools.GetZipArchives(filePath); }
            catch { throw new Exception(FILE_ERROR); }

            //Group by File extension
            List<IGrouping<string, ZipArchiveEntry>> groupExtensions;
            try { groupExtensions = ZipTools.GroupByExtension(zipArchive); }
            catch { throw new Exception(FILE_ERROR); }

            //Extract files
            try { ZipTools.ExtractZip(filePath, tempPath); }
            catch { throw new Exception(EXTRACT_ERROR); }

            //Total per extension
            foreach (IGrouping<string, ZipArchiveEntry> extension in groupExtensions)
                returnFileExtensions.Add(GetTotalPerExtension(extension));

            //Delete all temporary files
            try { FileTools.DeleteDirectory(tempPath); }
            catch { }

            //Repository totals
            return new ResponsePerRepository()
            {
                NameRepository = nameRepository,
                TotalBytes = returnFileExtensions.Select(a => a.TotalBytes).Sum(),
                TotalFiles = returnFileExtensions.Select(a => a.TotalFiles).Sum(),
                TotalLines = returnFileExtensions.Select(a => a.TotalLines).Sum(),
                PerExtension = returnFileExtensions
            };
        }

        internal ResponsePerExtension GetTotalPerExtension(IGrouping<string, ZipArchiveEntry> archives)
        {
            //Object of return
            ResponsePerExtension perExtension = new ResponsePerExtension();

            //Get the extension file
            perExtension.Extension = archives.Key;

            //Count of files with extension
            perExtension.TotalFiles = archives.ToList().Count();

            //Count of bytes of all files with extension
            perExtension.TotalBytes = archives.ToList().Select(a => a.Length).Sum();

            //Sum of lines of archives
            foreach (ZipArchiveEntry archive in archives.ToList())
                perExtension.TotalLines += File.ReadAllLines(Path.Combine(tempPath, archive.FullName)).Count();

            return perExtension;
        }
    }
}
