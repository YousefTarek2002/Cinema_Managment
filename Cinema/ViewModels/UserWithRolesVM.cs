using Cinema.Models;

namespace Cinema.ViewModels
{
    public class UserWithRolesVM
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }

}
