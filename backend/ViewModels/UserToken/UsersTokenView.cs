namespace backend.ViewModels;

public class UsersTokenViewModel {
    public IEnumerable<UserTokenViewModel> UserTokens { get; set; } = new List<UserTokenViewModel>();

    public PagingInfo PagingInfo { get; set; } = new PagingInfo();
}