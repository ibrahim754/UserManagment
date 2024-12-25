using System.Collections.Concurrent;
using System.Timers;
using UserManagement.Entites;
using UserManagement.Interfaces;

namespace UserManagement.Services
{
    public class PendingRegistrationService : IPendingRegistrationService
    {
        private readonly ConcurrentDictionary<string, PendingRegistration> _pendingRegistrations = new();
        private readonly System.Timers.Timer _cleanupTimer;

        public PendingRegistrationService()
        {
            _cleanupTimer = new System.Timers.Timer(60000); // Run every minute
            _cleanupTimer.Elapsed += CleanupExpiredRegistrations;
            _cleanupTimer.Start();
        }

        public void AddPendingRegistration(string email, PendingRegistration registration)
        {
            _pendingRegistrations[email] = registration;
        }

        public bool TryGetPendingRegistration(string email, out PendingRegistration registration)
        {
            return _pendingRegistrations.TryGetValue(email, out registration);
        }

        public void RemovePendingRegistration(string email)
        {
            _pendingRegistrations.TryRemove(email, out _);
        }

        private void CleanupExpiredRegistrations(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _pendingRegistrations
                .Where(kvp => kvp.Value.ExpiresOn < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _pendingRegistrations.TryRemove(key, out _);
            }
        }

        ~PendingRegistrationService()
        {
            _cleanupTimer?.Dispose();
        }
    }
}
