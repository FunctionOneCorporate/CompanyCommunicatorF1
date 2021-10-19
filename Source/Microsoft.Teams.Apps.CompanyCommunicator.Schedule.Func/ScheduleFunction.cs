// <copyright file="ScheduleFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Services;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure Function App triggered by messages from a Service Bus queue
    /// Used for sending messages from the bot.
    /// </summary>
    public class ScheduleFunction
    {
        /// <summary>
        /// This is set to 10 because the default maximum delivery count from the service bus
        /// message queue before the service bus will automatically put the message in the Dead Letter
        /// Queue is 10.
        /// </summary>
        private readonly INotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleFunction"/> class.
        /// </summary>
        /// <param name="notificationService">Notification Service function.</param>
        public ScheduleFunction(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        /// <summary>
        /// Azure Function App triggered by messages from a Service Bus queue
        /// Used for sending messages from the bot.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <param name="scheduleInitializationTimer">Schedule parameter.</param>
        /// <param name="log">Log parameter.</param>
        [FunctionName("ScheduleFunction")]
        public async Task RunAsync(
            [TimerTrigger("0 0 11 * * *", RunOnStartup = true)]
            TimerInfo scheduleInitializationTimer,
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: ");

            // F1
            try
            {
                var lstNotification = new List<ScheduleNotification>();

                log.LogInformation($"Definição de lista de Schedule");

                lstNotification = await this.notificationService.GetAllScheduleNotificationByDateAsync(DateTime.UtcNow);

                if (lstNotification != null)
                {
                    if (lstNotification.Count > 0)
                    {
                        foreach (var notificationEntity in lstNotification)
                        {
                            var resp = await this.notificationService.CreateSentNotificationToSchedule(notificationEntity);
                            log.LogInformation($"Send Result: {resp} in date: {DateTime.Now}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }
    }
}
