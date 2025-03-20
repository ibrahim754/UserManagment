using ErrorOr;

namespace UserManagement.Errors
{
    public static class UserErrors
    {
        public static readonly Error EmailAlreadyRegistered = Error.Validation(
            code: "User.EmailAlreadyRegistered",
            description: "Email is already registered.");

        public static readonly Error UsernameAlreadyRegistered = Error.Validation(
            code: "User.UsernameAlreadyRegistered",
            description: "UserName is already registered.");

        public static readonly Error ImageUploadFailed = Error.Failure(
            code: "User.ImageUploadFailed",
            description: "Failed to upload the user image.");

        public static readonly Error UserNotFound = Error.Validation(
            code: "User.UserNotFound",
            description: "User not found.");

        public static readonly Error InvalidCredentials  = Error.Failure(
            code: "User.InvalidCredentials ",
            description: "The password Or User Name is incorrect.");

        public static readonly Error IncorrectPassword = Error.Failure(
         code: "User.IncorrectPassword",
         description: "The password is incorrect."
         );

        public static readonly Error ChangePasswordFailed = Error.Failure(
            code: "User.ChangePasswordFailed",
            description: "Failed to change the password.");

        public static readonly Error FetchUsersFailed = Error.Failure(
            code: "User.FetchUsersFailed",
            description: "An error occurred while retrieving users.");

        public static readonly Error UserIsLockedOut = Error.Failure(
            code: "User.UserIsLockedOut",
            description: "User Is Blocked Due Too multiple failed Login");
        public static readonly Error LogInFailed = Error.Failure(
            code: "User.LogInFailed",
            description: "Log In Failed");
    }
}
