namespace backend.Utilities;

public static class ErrorMessage
{
    public static readonly Dictionary<String, String> ErrorMessages = new()
    {
        {"LoginError", "Login failed, please try again later"},//login error code and message
        {"SignUpError", "Sign Up failed, please try again later"},//signup error code and message
        {"LogoutError", "Logout failed, please try again later"},//logour errorb code and message
        {"Error", "An error occurred"},//general error code and message
        {"UsernameError", "The user with the provided username has not been found"},
        {"ValidationError", "Invalid data"},
        {"TokenError", "'s token not found"},
        {"UIDError", "The user with the following ID has not been: "}
    };

}