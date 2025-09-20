using Microsoft.AspNetCore.Identity;
using OzonDomains.Models;

namespace OzonRepositories.Context.Identity
{
    public class CustomIdentityUser :  IdentityUser
    {
        public int? UserAccessId { get; set; }
        public virtual UserAccess? UserAccess { get; set; }
    }
}

