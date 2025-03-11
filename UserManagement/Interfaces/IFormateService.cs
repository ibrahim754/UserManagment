using ErrorOr;

namespace UserManagement.Interfaces
{
    public interface IFormateService
    {
        ErrorOr<string> GenerateHtmlBody(string displayName, string content);

    }
}
