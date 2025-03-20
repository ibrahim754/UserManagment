using Microsoft.Extensions.Logging;
using Quartz;
using UserManagement.Interfaces;

namespace UserManagement.DAL.Configurations.Quartz
{
    [DisallowConcurrentExecution]
    public class RefreshTokenBackgroundJob : IJob
    {
        private readonly ILogger <RefreshTokenBackgroundJob> _logger;
        private readonly IMailService _mailService;

        public RefreshTokenBackgroundJob(ILogger<RefreshTokenBackgroundJob> logger,IMailService mailService)
        {
            _logger = logger;
            _mailService = mailService;
        }


        public Task Execute(IJobExecutionContext context)
        {
            //Fun();
            _logger.LogInformation("Esablish the refreshtoken background job at {DataTime.UtcNow}", DateTime.UtcNow);
            return Task.CompletedTask;
        }
        //private async Task Fun()
        //{
            
        //    var result = await _mailService.SendEmailAsync("ibrahimsalman277@gmail.com", "Test Quartz", "hello from the body");
        //    return;
        //}
    }
}
