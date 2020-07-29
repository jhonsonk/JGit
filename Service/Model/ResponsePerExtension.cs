namespace Service.Model
{
    public class ResponsePerExtension
    {
        public string Extension { get; set; }
        public long TotalLines { get; set; }
        public long TotalBytes { get; set; }
        public long TotalFiles { get; set; }
    }
}
