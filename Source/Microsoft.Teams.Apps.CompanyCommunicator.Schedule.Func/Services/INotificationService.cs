// <copyright file="INotificationService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.SendQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models;

    /// <summary>
    /// Notification service interface.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Checks if the send notification is throttled.
        /// </summary>
        /// <returns>true if the send notification is throttled, false otherwise.</returns>
        public Task<bool> IsSendNotificationThrottled();

        /// <summary>
        /// Checks if the notification is pending.
        /// </summary>
        /// <param name="message">Send Queue message.</param>
        /// <returns>true if the notification is pending, false otherwise.</returns>
        public Task<bool> IsPendingNotification(SendQueueMessageContent message);

        /// <summary>
        /// Set SendNotification Throttled.
        /// </summary>
        /// <param name="sendRetryDelayNumberOfSeconds">Send Retry delay.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task SetSendNotificationThrottled(double sendRetryDelayNumberOfSeconds);

        /// <summary>
        /// Updates sent notification for the recipient.
        /// </summary>
        /// <param name="notificationId">The notification Id.</param>
        /// <param name="recipientId">The recipient's unique identifier.
        ///     If the recipient is a user, this should be the AAD Id.
        ///     If the recipient is a team, this should be the team Id.</param>
        /// <param name="totalNumberOfSendThrottles">The total number of throttled requests to send the notification.</param>
        /// <param name="statusCode">Status code.</param>
        /// <param name="allSendStatusCodes">A comma separated list representing all of the status code responses received when trying
        /// to send the notification to the recipient.</param>
        /// <param name="errorMessage">The error message to store in the database.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task UpdateSentNotification(
            string notificationId,
            string recipientId,
            int totalNumberOfSendThrottles,
            int statusCode,
            string allSendStatusCodes,
            string errorMessage);

        /// <summary>
        /// Create notification schedule in table NotificationData.
        /// </summary>
        /// <param name="scheduleNotification">The notification Id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<bool> CreateSentNotificationToSchedule(ScheduleNotification scheduleNotification);

        /// <summary>
        /// Get all schedule for date.
        /// </summary>
        /// <param name="date">Date parameter.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<ScheduleNotification>> GetAllScheduleNotificationByDateAsync(DateTime date);

        /// <summary>
        /// Get sent notification in channel the Teams.
        /// </summary>
        /// <param name="messageIdTeams"> Id message in Microsoft Teams. </param>
        /// <param name="conversationId"> Channel posted notification. </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<SentNotificationData> GetSentNotificationforMessageIdTeams(string messageIdTeams, string conversationId);
    }
}
