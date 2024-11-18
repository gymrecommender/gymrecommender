using backend.Enums;

namespace backend.DTO;

public class AccountDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Provider { get; set; }
    public string OuterUid { get; set; }
    public bool IsEmailVerified { get; set; }
}