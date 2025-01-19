using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models.DTOs
{
    public class InstitutionDTO
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string ContactEmail { get; set; }
        [Required]
        public bool IsPublic { get; set; }

        public InstitutionDTO()
        {
            
        }

        public InstitutionDTO(string name, string address, string contactEmail, bool isPublic)
        {
            Name = name;
            Address = address;
            ContactEmail = contactEmail;
            IsPublic = isPublic;
        }

        public InstitutionDTO(Institution inst)
            :this(inst.Name, inst.Address, inst.ContactEmail, inst.IsPublic)
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
