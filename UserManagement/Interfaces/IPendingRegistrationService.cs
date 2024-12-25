
using UserManagement.Entites;

namespace UserManagement.Interfaces
{
    public interface IPendingRegistrationService
    {
        void AddPendingRegistration(string email, PendingRegistration registration);
        bool TryGetPendingRegistration(string email, out PendingRegistration registration);
        void RemovePendingRegistration(string email);
    }
}
