namespace backend.Utilities;

public static class ErrorMessage
{
    public static readonly Dictionary<String, String> ErrorMessages = new()
    {
        {"LoginError", "Login failed."},//login error code and message
        {"SignUpError", "SignUp failed."},//signup error code and message
        {"LogoutError", "Logout failed."},//logour errorb code and message
        {"Error", "Error."},//general error code and message
        {"UsernameError", "User not found."},
        {"ValidationError", "Invalid data."},
        {"TokenError", "'s token not found"},
        {"UIDError", "Dindn't find user with UID: "}
    };

}