using ErrorOr;

namespace UserManagement.Errors
{
    public static class TokenErrors
    {
        public static readonly Error InvalidToken = Error.Validation(
            code: "Token.InvalidToken",
            description: "Invalid token.");

        public static readonly Error RefreshTokenNotFound = Error.Validation(
            code: "Token.RefreshTokenNotFound",
            description: "Refresh token not found.");

        public static readonly Error InactiveToken = Error.Validation(
            code: "Token.InactiveToken",
            description: "Token is already inactive.");

        public static readonly Error TokenRevokeFailed = Error.Failure(
            code: "Token.TokenRevokeFailed",
            description: "Failed to revoke token due to an internal error.");
    }
}
