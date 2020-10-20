using System.Collections.Generic;

namespace Service.Model
{
    public class ResponsePerRepository
    {
        public string NameRepository { get; set; }
        public string Commit { get; set; }
        public long TotalLines { get; set; }
        public long TotalBytes { get; set; }
        public long TotalFiles { get; set; }
        public List<ResponsePerExtension> PerExtension { get; set; }
    }
}
