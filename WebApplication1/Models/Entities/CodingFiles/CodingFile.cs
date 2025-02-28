using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities.CodingFiles
{
    public class CodingFile : ICloneable
    {
        [Key]
        public int Id { get; set; }
    
        [Required]
        public string Name { get; set; }
    
        [Required]
        public string Type { get; set; }

        public byte[] Data { get; set; }

        [ForeignKey("Folder")]
        public int FolderId { get; set; }

        //Nav props
        public CodingFolder Folder { get; set; }

        public object Clone()
        {
            return new CodingFile()
            {
                Name = Name,
                Type = Type,
                Data = Data,
                FolderId = FolderId
            };
        }
    }
}
