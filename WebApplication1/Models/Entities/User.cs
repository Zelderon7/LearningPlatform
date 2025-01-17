using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models.Entities
{
    public class User: IdentityUser<int>
    {
        public string Role { get; set; } = "STUDENT";
        public string PictureUrl { get; set; } = "";
    }
}
