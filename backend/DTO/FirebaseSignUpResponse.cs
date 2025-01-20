namespace backend.DTO;

public class FirebaseSignUpResponse
{
    public string idToken { get; set; }
    public string email { get; set; }
    public string refreshToken { get; set; }
    public string expiresIn { get; set; }
    public string localId { get; set; }
    public string kind { get; set; }
}