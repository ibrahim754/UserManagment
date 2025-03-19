using Microsoft.Extensions.Options;
using Quartz;
using UserManagement.DAL.Configurations.Quartz;

namespace UserManagement.DAL.Configurations.Quartz
{
    public class RefreshTokenBackgroundJobSetup : IConfigureOptions<QuartzOptions>
    {
        public void Configure(QuartzOptions options)
        {
            var jobKey = JobKey.Create(nameof(RefreshTokenBackgroundJob));

            options
                .AddJob<RefreshTokenBackgroundJob>(jobBuilder => jobBuilder.WithIdentity(jobKey))
                .AddTrigger(trigger =>
                    trigger
                        .ForJob(jobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule.WithIntervalInMinutes(30)
                            .RepeatForever()
                            ));
        }
    }
}
