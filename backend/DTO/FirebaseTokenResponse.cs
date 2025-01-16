namespace backend.DTO;

public class FirebaseTokenResponse {
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }
    public string ExpiresIn { get; set; }
}