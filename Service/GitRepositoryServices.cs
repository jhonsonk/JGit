using HtmlAgilityPack;
using Service.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace Service
{
    public class GitRepositoryServices
    {
        #region Mensagens
        private string DOWNLOAD_ERROR = "The repository is invalid or not found";
        private string ERROR = "An error has occurred when getting the repository";
        #endregion


        private string tempPath = "";
        public GitRepositoryServices()
        {
            tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "temp", Guid.NewGuid().ToString());
        }

        //List<GitRow> returnsa = new List<GitRow>();

        ///Obter a pagina 
        public List<GitRow> GetNodesRowHeader(string url,  List<GitRow> returns)
        {
            WebClient client = new WebClient();
            Stream data = client.OpenRead(new Uri(url));
            StreamReader reader = new StreamReader(data);
            string htmlContent = reader.ReadToEnd();
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            List<HtmlNode> nodeRoot = document.DocumentNode.SelectNodes("//div[@role=\"row\"]").ToList().Where(a => a.GetClasses().Contains("Box-row")).ToList();

            Task<List<GitRow>> task = null;
            foreach (HtmlNode node in nodeRoot)
            {
                GitRow rowFile = new GitRow();
                HtmlNode item = node.ChildNodes.SelectMany(a => a.Attributes).Where(a => a.Value == "rowheader").FirstOrDefault().OwnerNode;
                HtmlNode itemSpan = item.ChildNodes.ToList().Where(a => a.Name == "span").FirstOrDefault();

                if (itemSpan == null)
                    continue;

                rowFile.name = itemSpan.InnerText;
                rowFile.url = itemSpan.ChildNodes[0].GetAttributes().Where(a => a.Name == "href").FirstOrDefault().Value;

                if (node.ChildNodes.SelectMany(a => a.Attributes).SelectMany(a => a.OwnerNode.ChildNodes).SelectMany(a => a.Attributes).Where(a => a.Value == "Directory").ToList().Count > 1)
                {
                    rowFile.isDirectory = true;
                    task = Task.Factory.StartNew(() => GetNodesRowHeader("https://github.com/" + rowFile.url, returns));                    
                    continue;
                }
                returns.Add(rowFile);
            }
            try
            {
                task.Wait();
            }
            catch { 
            }
            return returns;
        }

        //public List<GitRow> agag(List<GitRow> gitRows, List<GitRow> returns)
        //{

        //    foreach (GitRow item in gitRows)
        //    {
        //        if (item.isDirectory)
        //        {
        //            List<GitRow> root = GetNodesRowHeader("https://github.com/" + item.url);
        //            agag(root, returns);
        //            continue;
        //        }
        //        returns.Add(item);
        //    }

        //    return returns;
        //}

        public List<GitRow> GetTotalaAsync(string gitUrl)
        {
            List<GitRow> returnoa = new List<GitRow>();
            try
            {
                GetNodesRowHeader(gitUrl, returnoa);

            }
            catch (Exception e)
            {
                throw new Exception(ERROR);
            }

            return returnoa;
        }

        public class GitRow
        {
            public string name { get; set; }
            public string url { get; set; }
            public bool isDirectory { get; set; }
            public decimal tamanho { get; set; }

        }
    }
}
