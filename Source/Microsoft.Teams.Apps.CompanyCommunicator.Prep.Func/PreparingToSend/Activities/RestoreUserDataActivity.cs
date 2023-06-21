using Microsoft.Graph;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
using Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend;
using Polly;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend.Activities
{
    public static async class RestoreUserDataActivity
    {
        GetRecipientsActivity getRecipientsActivity = new GetRecipientsActivity();
        var recipients =  
        notification);

        var count = recipients.Count();
            if (!context.IsReplaying)
            {
                log.LogInformation($"About to create conversation with {count} recipients.");
            }

            if (count > 0)
            {
                // Update notification status.
                await context.CallActivityWithRetryAsync(
                    FunctionNames.UpdateNotificationStatusActivity,
                    FunctionSettings.DefaultRetryOptions,
                    (notification.Id, NotificationStatus.InstallingApp));
}

// Create conversation.
var tasks = new List<Task>();
foreach (var recipient in recipients)
{
    var task = context.CallActivityWithRetryAsync(
        FunctionNames.TeamsConversationActivity,
        FunctionSettings.DefaultRetryOptions,
        (notification.Id, recipient));
    tasks.Add(task);
}

// Fan-out Fan-in.
await Task.WhenAll(tasks);
    }
}
