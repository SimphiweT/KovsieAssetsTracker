namespace KovsieAssetsTracker.WebAppMVC.Models.User
{
    public class CreateAndListUserViewModel
    {
        public CreateUserViewModel CreateUser { get; set; } = new CreateUserViewModel();
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
    }
}
