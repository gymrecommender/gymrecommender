namespace backend.ViewModels;

public class UserTokenViewModel {
    public Guid Id { get; set; }

    public IEnumerable<AccountViewModel> User { get; set; } = new List<AccountViewModel>();

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}