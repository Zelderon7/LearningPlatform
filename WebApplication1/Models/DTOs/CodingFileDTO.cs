using System.Text;
using WebApplication1.Models.Entities.CodingFiles;

namespace WebApplication1.Models.DTOs
{
    public class CodingFileDTO
    {
        public string Name { get; set; }
        public string Extention { get; set; }
        public string FullName 
        { 
            get => Name + Extention;
            set
            {
                string[] a = value.Split('.');
                if (a.Length != 2)
                    throw new ArgumentException();

                Name = a[0];
                Extention = "." + a[1];
            }
        }
        public string Data { get; set; }

        public CodingFileDTO()
        {

        }

        public CodingFileDTO(string name, string extention, string data)
        {
            Name = name;
            Extention = extention;
            Data = data;
        }

        public CodingFileDTO(CodingFile file)
        {
            Name = file.Name;
            Extention = file.Type;
            Data = Encoding.UTF8.GetString(file.Data);
        }
    }
}
