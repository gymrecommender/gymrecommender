namespace backend.DTO;

public class AccountPwdUpdateDto {
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }   
}