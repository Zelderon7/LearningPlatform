using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models.Entities
{
    public class User: IdentityUser
    {
        public string Role { get; set; } = "STUDENT";
        public string PictureUrl { get; set; } = "";
    }
}
