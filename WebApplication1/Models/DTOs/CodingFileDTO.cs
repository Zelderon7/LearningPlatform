using System.Text;
using System.Text.Json.Serialization;
using WebApplication1.Models.Entities.CodingFiles;

namespace WebApplication1.Models.DTOs
{
    public class CodingFileDTO
    {
        [JsonInclude]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Extention { get; set; }
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
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

        public CodingFileDTO(int id, string name, string extention, string data)
        {
            Id = id;
            Name = name;
            Extention = extention;
            Data = data;
        }

        public CodingFileDTO(CodingFile file)
        {
            Id = file.Id;
            Name = file.Name;
            Extention = file.Type;
            Data = Encoding.UTF8.GetString(file.Data);
        }
    }
}
