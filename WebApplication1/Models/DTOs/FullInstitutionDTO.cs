using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models.DTOs
{
    public class FullInstitutionDTO
    {
        public int? Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string ContactEmail { get; set; }

        [MaxLength(6)]
        public string Code { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        public string? ImageUrl { get; set; }

        public Class[]? Classes { get; set; }

        public FullInstitutionDTO()
        {
            
        }

        public FullInstitutionDTO(int? id, string name, string address, string contactEmail, string code, bool isPublic, string? imageUrl, Class[]? classes)
        {
            Id = id;
            Name = name;
            Address = address;
            ContactEmail = contactEmail;
            IsPublic = isPublic;
            Code = code;
            ImageUrl = imageUrl;
            Classes = classes;
        }

        public FullInstitutionDTO(Institution inst)
            :this(inst.InstitutionId, inst.Name, inst.Address, inst.ContactEmail, inst.Code, inst.IsPublic, inst.ImageUrl, inst.Classes.ToArray())
        {
            
        }

        public Institution ToInstitution()
        {
            return new Institution
            {
                Name = Name,
                Address = Address,
                ContactEmail = ContactEmail,
                IsPublic = IsPublic
            };
        }
    }
}
