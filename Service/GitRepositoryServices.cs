﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using Service.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Service
{
    public class GitRepositoryServices
    {
        private string tempPath = "";
        public GitRepositoryServices()
        {
            tempPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "temp");
        }

        public ResponsePerRepository GetTotal(string gitUrl)
        {
            ResponsePerRepository responsePerRepository = GetInformationGit(gitUrl);

            if (File.Exists(Path.Combine(tempPath, responsePerRepository.Commit)))
            {
                string stringFile = File.ReadAllText(Path.Combine(tempPath, responsePerRepository.Commit));
                ResponsePerRepository recovery = JsonConvert.DeserializeObject<ResponsePerRepository>(stringFile);
                return recovery;
            }

            FileTools.SaveFile(responsePerRepository.Commit, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(responsePerRepository)), tempPath);

            Task task = Task.Run(() => GetTotalProcessing(responsePerRepository, gitUrl));
            task.Wait(28000);

            return responsePerRepository;
        }

        internal ResponsePerRepository GetTotalProcessing(ResponsePerRepository responsePerRepository, string gitUrl)
        {
            try
            {
                List<GitRow> gitRows = new List<GitRow>();

                GetNodesRowHeader(gitUrl, gitRows);

                List<Task> tasks = new List<Task>();
                foreach (var item in gitRows)
                {
                    Task task = Task.Run(() => GetFileInformation(item));
                    tasks.Add(task);
                }

                Task.WaitAll(tasks.ToArray());

                responsePerRepository.PerExtension = GetTotalPerGroupExtension(gitRows);
                responsePerRepository.TotalBytes = responsePerRepository.PerExtension.Select(a => a.TotalBytes).Sum();
                responsePerRepository.TotalLines = responsePerRepository.PerExtension.Select(a => a.TotalLines).Sum();
                responsePerRepository.TotalFiles = responsePerRepository.PerExtension.Select(a => a.TotalFiles).Sum();
                responsePerRepository.Status = "COMPLETE";
                FileTools.SaveFile(responsePerRepository.Commit, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(responsePerRepository)), tempPath);
                Console.WriteLine("Save: " + responsePerRepository.Commit);
                return responsePerRepository;

            }
            catch (Exception e)
            {
                string json = JsonConvert.SerializeObject(e, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                });

                throw new Exception(json);
            }
        }

        internal ResponsePerRepository GetInformationGit(string url)
        {
            ResponsePerRepository responsePerRepository = new ResponsePerRepository();

            WebClient client = new WebClient();
            Stream data = client.OpenRead(new Uri(url));
            StreamReader reader = new StreamReader(data);
            string htmlContent = reader.ReadToEnd();
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            responsePerRepository.Commit = document.DocumentNode.SelectNodes("//a[@class=\"d-none js-permalink-shortcut\"]").ToList().SelectMany(a => a.Attributes).Where(a => a.Name == "href").FirstOrDefault().Value;
            responsePerRepository.NameRepository = document.DocumentNode.SelectNodes("//a[@data-pjax=\"#js-repo-pjax-container\"]").ToList().SelectMany(a => a.Attributes).Where(a => a.Name == "href").FirstOrDefault().Value;
            responsePerRepository.Commit = responsePerRepository.Commit.Split('/')[responsePerRepository.Commit.Split('/').Length - 1];
            responsePerRepository.Status = "PROCESSING";
            return responsePerRepository;
        }

        internal List<GitRow> GetNodesRowHeader(string url, List<GitRow> returns)
        {
            WebClient client = new WebClient();
            Stream data = client.OpenRead(new Uri(url));
            StreamReader reader = new StreamReader(data);
            string htmlContent = reader.ReadToEnd();
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            List<HtmlNode> nodeRoot = document.DocumentNode.SelectNodes("//div[@role=\"row\"]").ToList().Where(a => a.GetClasses().Contains("Box-row")).ToList();

            List<Task> tasks = new List<Task>();
            foreach (HtmlNode node in nodeRoot)
            {
                Task task = Task.Run(() => GetNode(node, returns));
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

            return returns;
        }

        internal void GetNode(HtmlNode node, List<GitRow> gitRows)
        {
            GitRow rowFile = new GitRow();
            HtmlNode item = node.ChildNodes.SelectMany(a => a.Attributes).Where(a => a.Value == "rowheader").FirstOrDefault().OwnerNode;
            HtmlNode itemSpan = item.ChildNodes.ToList().Where(a => a.Name == "span").FirstOrDefault();

            if (itemSpan == null)
                return;

            rowFile.Name = itemSpan.InnerText;
            rowFile.Url = itemSpan.ChildNodes[0].GetAttributes().Where(a => a.Name == "href").FirstOrDefault().Value;
            Console.WriteLine(rowFile.Url);

            if (node.ChildNodes.SelectMany(a => a.Attributes).SelectMany(a => a.OwnerNode.ChildNodes).SelectMany(a => a.Attributes).Where(a => a.Value == "Directory").ToList().Count > 1)
            {
                rowFile.IsDirectory = true;
                GetNodesRowHeader("https://github.com/" + rowFile.Url, gitRows);
            }
            else
            {
                rowFile = GetFileInformation(rowFile);
                gitRows.Add(rowFile);
            }
        }

        internal GitRow GetFileInformation(GitRow row)
        {
            try
            {
                var result = Task.Run(() => DownloadTools.DownloadFile("https://raw.githubusercontent.com/" + row.Url.Replace("blob", ""))).Result;
                string converted = Encoding.UTF8.GetString(result, 0, result.Length);
                row.Length = result.Length;
                row.Extension = row.Name.Split('.')[row.Name.Split('.').Length - 1];
                row.TotalLines = converted.Split('\n').Length;
                return row;
            }
            catch
            {
                return row;
            }
        }

        internal List<ResponsePerExtension> GetTotalPerGroupExtension(List<GitRow> listRows)
        {
            IEnumerable<IGrouping<string, GitRow>> groupByExtension = listRows.GroupBy(a => a.Extension);
            List<ResponsePerExtension> listReturn = new List<ResponsePerExtension>();
            foreach (var item in groupByExtension)
            {
                listReturn.Add(GetTotalPerExtension(item));
            }
            return listReturn;
        }

        internal ResponsePerExtension GetTotalPerExtension(IGrouping<string, GitRow> GitRows)
        {
            ResponsePerExtension perExtension = new ResponsePerExtension();
            perExtension.Extension = GitRows.Key;
            perExtension.TotalFiles = GitRows.ToList().Count();
            perExtension.TotalBytes = GitRows.ToList().Select(a => a.Length).Sum();
            perExtension.TotalLines = GitRows.ToList().Select(a => a.TotalLines).Sum();
            return perExtension;
        }
    }
}
