using backend.Enums;

namespace backend.ViewModels;

public class AccountViewModel
{
    public Guid Id { get; set; }
    
    public string Username { get; set; } = null!;
    
    public string OuterUid { get; set; } = null!;
    
    public string Email { get; set; } = null!;
    
    public bool IsEmailVerified { get; set; }
    
    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastSignIn { get; set; }
    
    public string PasswordHash { get; set; } = null!;
    
    public AccountType Type { get; set; }
    
    public ProviderType Provider { get; set; }
    
    public ICollection<AccountViewModel> CreatedBy { get; set; } = new List<AccountViewModel>();
}