namespace Cinema.ViewModels
{
    public class RoleCheckboxItem
    {
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }

    public class UserRolesVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<RoleCheckboxItem> Roles { get; set; } = new();
    }
}
