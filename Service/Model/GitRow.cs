using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Model
{
    public class GitRow
    {
        public string Extension { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsDirectory { get; set; }
        public int Length { get; set; }
        public int TotalLines { get; set; }
    }
}
