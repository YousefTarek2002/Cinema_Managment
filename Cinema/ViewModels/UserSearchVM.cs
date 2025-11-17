namespace Cinema.ViewModels
{
    public class UserSearchVM
    {
        public string Query { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

}
