namespace WebApplication1.Models.DTOs
{
    public class CodeExecutionResponse
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public string[] Files { get; set; }
    }
}
