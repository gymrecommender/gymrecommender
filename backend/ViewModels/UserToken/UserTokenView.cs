namespace backend.ViewModels;

public class UserTokenViewModel {
    public Guid Id { get; set; }

    public IEnumerable<AccountRegularModel> User { get; set; } = new List<AccountRegularModel>();

    public string Token { get; set; } = null!;
}