using Microsoft.AspNetCore.Identity;
using OzonDomains.Models;
using OzonRepositories.Context.Identity;
using Servcies.FiltersServcies.FilterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servcies.FiltersServcies.DataFilterManagers
{
    public class UserFilterManager : IFilterManagerAsync<CustomIdentityUser, UserFilterModel>
    {
        private readonly DataFilter<CustomIdentityUser> _filter;
        private readonly UserManager<CustomIdentityUser> _userManager;

        public UserFilterManager(DataFilter<CustomIdentityUser> filter, UserManager<CustomIdentityUser> userManager) 
        { 
            _filter = filter;
            _userManager = userManager;
        }

        public async Task<List<CustomIdentityUser>> FilterByFilterData(List<CustomIdentityUser> users, UserFilterModel filterData)
        {
            // Фильтрация по имени пользователя
            users = _filter.FilterByString(users, u => u.UserName, filterData.UserName).ToList();

            // Фильтрация по доступу пользователя
            users = _filter.FilterByString(users, u => u.UserAccess != null ? u.UserAccess.Name : "", filterData.UserAccess?.Name).ToList();

            // Фильтрация по роли пользователя
            if (!string.IsNullOrEmpty(filterData.UserRole))
            {
                var filteredUsers = new List<CustomIdentityUser>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains(filterData.UserRole))
                    {
                        filteredUsers.Add(user);
                    }
                }

                users = filteredUsers;
            }

            return users;
        }


        public Task<List<CustomIdentityUser>> FilterByRadioButton(List<CustomIdentityUser> filterModel, string buttonState)
        {
            throw new NotImplementedException();
        }
    }
}
