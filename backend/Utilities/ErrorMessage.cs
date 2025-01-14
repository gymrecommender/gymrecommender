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
        {"UIDError", "The user with the following ID has not been found: "},
        {"Internal error", "Internal error occured"},
        {"Location error", "Unknown city or country"},
        {"OwnedGymError", "The gym with the provided id is not found or you do not have the rights to manage the requested gym"}
    };

}