using backend.Enums;

namespace backend.ViewModels;

public class AccountRegularModel {
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsEmailVerified { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime? LastSignIn { get; set; }

    public string Type { get; set; } = null!;

    public string Provider { get; set; } = null!;
}