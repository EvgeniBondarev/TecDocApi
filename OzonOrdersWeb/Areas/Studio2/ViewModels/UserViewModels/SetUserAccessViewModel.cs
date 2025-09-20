using OzonDomains.Models;
using OzonRepositories.Context.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonOrdersWeb.ViewModels.UserViewModels
{
    public class SetUserAccessViewModel
    {
        public CustomIdentityUser User { get; set; }
        public List<UserAccess> UserAccesses { get; set; }
        public int SelectedUserAccessId { get; set; }
        public string UserId { get; set; }
    }
}
