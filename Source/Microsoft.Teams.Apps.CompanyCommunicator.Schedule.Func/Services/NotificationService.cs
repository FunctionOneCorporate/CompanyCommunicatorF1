// <copyright file="NotificationService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Extensions;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamsRatingAnalytics;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.DataQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.PrepareToSendQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.SendQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models;

    /// <summary>
    /// Notification Service.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly GlobalSendingNotificationDataRepository globalSendingNotificationDataRepository;
        private readonly SentNotificationDataRepository sentNotificationDataRepository;
        private readonly NotificationDataRepository notificationDataRepository;
#pragma warning disable CS0649 // Field 'NotificationService.forceCompleteMessageDelayInSeconds' is never assigned to, and will always have its default value 0
        private readonly double forceCompleteMessageDelayInSeconds;
#pragma warning restore CS0649 // Field 'NotificationService.forceCompleteMessageDelayInSeconds' is never assigned to, and will always have its default value 0
        private readonly PrepareToSendQueue prepareToSendQueue;
        private readonly DataQueue dataQueue;
        private readonly ILogger<NotificationService> logger;
        private readonly TeamsRatingAnalyticsDataRepository teamsRatingAnalyticsDataRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="globalSendingNotificationDataRepository">The global sending notification data repository.</param>
        /// <param name="sentNotificationDataRepository">The sent notification data repository.</param>
        /// <param name="notificationDataRepository">The notification data repository.</param>
        /// <param name="prepareToSendQueue">Queue for sent messages.</param>
        /// <param name="dataQueue">Data message.</param>
        /// <param name="loggerFactory">Log operation.</param>
        /// <param name="teamsRatingAnalyticsDataRepository">Then rating data repository.</param>
        public NotificationService(
            GlobalSendingNotificationDataRepository globalSendingNotificationDataRepository,
            SentNotificationDataRepository sentNotificationDataRepository,
            NotificationDataRepository notificationDataRepository,
            PrepareToSendQueue prepareToSendQueue,
            DataQueue dataQueue,
            ILoggerFactory loggerFactory,
            TeamsRatingAnalyticsDataRepository teamsRatingAnalyticsDataRepository)
        {
            this.globalSendingNotificationDataRepository = globalSendingNotificationDataRepository ?? throw new ArgumentNullException(nameof(globalSendingNotificationDataRepository));
            this.sentNotificationDataRepository = sentNotificationDataRepository ?? throw new ArgumentNullException(nameof(sentNotificationDataRepository));
            this.notificationDataRepository = notificationDataRepository ?? throw new ArgumentNullException(nameof(notificationDataRepository));
            this.prepareToSendQueue = prepareToSendQueue ?? throw new ArgumentException(nameof(prepareToSendQueue));
            this.dataQueue = dataQueue ?? throw new ArgumentException(nameof(dataQueue));
            this.logger = loggerFactory?.CreateLogger<NotificationService>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.teamsRatingAnalyticsDataRepository = teamsRatingAnalyticsDataRepository ?? throw new ArgumentException(nameof(teamsRatingAnalyticsDataRepository));
        }

        /// <inheritdoc/>
        public async Task<bool> IsSendNotificationThrottled()
        {
            var globalNotificationStatus = await this.globalSendingNotificationDataRepository.GetGlobalSendingNotificationDataEntityAsync();
            if (globalNotificationStatus?.SendRetryDelayTime == null)
            {
                return false;
            }

            return globalNotificationStatus.SendRetryDelayTime > DateTime.UtcNow;
        }

        /// <inheritdoc/>
        public async Task<bool> IsPendingNotification(SendQueueMessageContent message)
        {
            var recipient = message?.RecipientData;
            if (string.IsNullOrWhiteSpace(recipient?.RecipientId))
            {
                throw new InvalidOperationException("Recipient id is not set.");
            }

            // Check notification status for the recipient.
            var notification = await this.sentNotificationDataRepository.GetAsync(
                partitionKey: message.NotificationId,
                rowKey: message.RecipientData.RecipientId);

            // To avoid sending duplicate messages, we check if the Status code is either of the following:
            // 1. InitializationStatusCode: this means the notification has not been attempted to be sent to this recipient.
            // 2. FaultedAndRetryingStatusCode: this means the Azure Function previously attempted to send the notification
            //    to this recipient but failed and should be retried.
            if (notification?.StatusCode == SentNotificationDataEntity.InitializationStatusCode ||
                notification?.StatusCode == SentNotificationDataEntity.FaultedAndRetryingStatusCode)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task SetSendNotificationThrottled(double sendRetryDelayNumberOfSeconds)
        {
            // Ensure global retry timestamp is less re-queue delay time for the message.
            var globalSendingNotificationDataEntity = new GlobalSendingNotificationDataEntity
            {
                SendRetryDelayTime = DateTime.UtcNow + TimeSpan.FromSeconds(sendRetryDelayNumberOfSeconds - 15),
            };

            await this.globalSendingNotificationDataRepository
                .SetGlobalSendingNotificationDataEntityAsync(globalSendingNotificationDataEntity);
        }

        /// <inheritdoc/>
        public async Task UpdateSentNotification(
            string notificationId,
            string recipientId,
            int totalNumberOfSendThrottles,
            int statusCode,
            string allSendStatusCodes,
            string errorMessage)
        {
            // Current time as sent date time.
            var sentDateTime = DateTime.UtcNow;

            var notification = await this.sentNotificationDataRepository.GetAsync(
                partitionKey: notificationId,
                rowKey: recipientId);

            // Update notification.
            notification.TotalNumberOfSendThrottles = totalNumberOfSendThrottles;
            notification.SentDate = sentDateTime;
            notification.IsStatusCodeFromCreateConversation = false;
            notification.StatusCode = (int)statusCode;
            notification.ErrorMessage = errorMessage;
            notification.NumberOfFunctionAttemptsToSend = notification.NumberOfFunctionAttemptsToSend + 1;
            notification.AllSendStatusCodes = $"{notification.AllSendStatusCodes ?? string.Empty}{allSendStatusCodes}";

            if (statusCode == (int)HttpStatusCode.Created)
            {
                notification.DeliveryStatus = SentNotificationDataEntity.Succeeded;
            }
            else if (statusCode == (int)HttpStatusCode.TooManyRequests)
            {
                notification.DeliveryStatus = SentNotificationDataEntity.Throttled;
            }
            else if (statusCode == (int)HttpStatusCode.NotFound)
            {
                notification.DeliveryStatus = SentNotificationDataEntity.RecipientNotFound;
            }
            else if (statusCode == SentNotificationDataEntity.FaultedAndRetryingStatusCode)
            {
                notification.DeliveryStatus = SentNotificationDataEntity.Retrying;
            }
            else
            {
                notification.DeliveryStatus = SentNotificationDataEntity.Failed;
            }

            await this.sentNotificationDataRepository.InsertOrMergeAsync(notification);
        }

        /// <inheritdoc/>
        public async Task<bool> CreateSentNotificationToSchedule(ScheduleNotification scheduleNotification)
        {
            var scheduleNotificationDataEntity = await this.notificationDataRepository.GetAsync(NotificationDataTableNames.ScheduleNotificationsPartition, scheduleNotification.Id);

            if (scheduleNotificationDataEntity == null)
            {
                return false;
            }

            var strNmMessage = scheduleNotificationDataEntity.NmMensagem;

            var newSentNotificationId = await this.notificationDataRepository.MoveScheduleToSentPartitionAsync(scheduleNotificationDataEntity);

            await this.sentNotificationDataRepository.EnsureSentNotificationDataTableExistsAsync();

            // Insert data in Table TeamsRatingDataRepository
            await this.teamsRatingAnalyticsDataRepository.CreateTeamsRatingAnalyticsData(newSentNotificationId, strNmMessage);

            var prepareToSendQueueMessageContent = new PrepareToSendQueueMessageContent
            {
                NotificationId = newSentNotificationId,
            };

            await this.prepareToSendQueue.SendAsync(prepareToSendQueueMessageContent);

            var forceCompleteDataQueueMessageContent = new DataQueueMessageContent
            {
                NotificationId = newSentNotificationId,
                ForceMessageComplete = true,
            };

            await this.dataQueue.SendDelayedAsync(forceCompleteDataQueueMessageContent, this.forceCompleteMessageDelayInSeconds);

            return true;
        }

        /// <inheritdoc/>
        public async Task<List<ScheduleNotification>> GetAllScheduleNotificationByDateAsync(DateTime date)
        {
            var notificationEntities = await this.notificationDataRepository.GetAllScheduleNotificationsAsync();

            var lista = new List<ScheduleNotification>();
            foreach (var notificationEntity in notificationEntities)
            {
                if (date >= Convert.ToDateTime(notificationEntity.ScheduleDate))
                {
                    var summary = new ScheduleNotification
                    {
                        Id = notificationEntity.Id,
                        Title = notificationEntity.Title,
                        ImageLink = notificationEntity.ImageLink,
                        Summary = notificationEntity.Summary,
                        Author = notificationEntity.Author,
                        ButtonTitle = notificationEntity.ButtonTitle,
                        ButtonLink = notificationEntity.ButtonLink,
                        CreatedDateTime = notificationEntity.CreatedDate,
                        Teams = notificationEntity.Teams,
                        Rosters = notificationEntity.Rosters,
                        Groups = notificationEntity.Groups,
                        AllUsers = notificationEntity.AllUsers,
                        Schedule = notificationEntity.Schedule,
                        ScheduleDate = notificationEntity.ScheduleDate,
                        NmMensagem = notificationEntity.NmMensagem,
                        HeaderImgLink = notificationEntity.HeaderImgLink,
                        FooterImgLink = notificationEntity.FooterImgLink,
                        ButtonTitle2 = notificationEntity.ButtonTitle2,
                        ButtonLink2 = notificationEntity.ButtonLink2,
                    };

                    lista.Add(summary);
                }
            }

            return lista;
        }

        /// <inheritdoc/>
        public async Task<SentNotificationData> GetSentNotificationforMessageIdTeams(string messageIdTeams, string conversationId)
        {
            SentNotificationData sentNotification = new SentNotificationData();
            try
            {
                string strParameters = $"ConversationId eq '{conversationId}' and MessageTeamsId eq '{messageIdTeams}'";
                var sentNotificationEntities = await this.sentNotificationDataRepository.GetWithFilterAsync2(strParameters);

                if (sentNotificationEntities != null)
                {
                    foreach (var sent in sentNotificationEntities)
                    {
                        sentNotification.PartitionKey = sent.PartitionKey;
                        sentNotification.RowKey = sent.RowKey;
                        sentNotification.Timestamp = sent.Timestamp;
                        sentNotification.AllSendStatusCodes = sent.AllSendStatusCodes;
                        sentNotification.ConversationId = sent.ConversationId;
                        sentNotification.DeliveryStatus = sent.DeliveryStatus;
                        sentNotification.IsStatusCodeFromCreateConversation = sent.IsStatusCodeFromCreateConversation;
                        sentNotification.MessageTeamsId = sent.MessageTeamsId;
                        sentNotification.NumberOfFunctionAttemptsToSend = sent.NumberOfFunctionAttemptsToSend;
                        sentNotification.RecipientId = sent.RecipientId;
                        sentNotification.RecipientType = sent.RecipientType;
                        sentNotification.SentDate = sent.SentDate;
                        sentNotification.ServiceUrl = sent.ServiceUrl;
                        sentNotification.StatusCode = sent.StatusCode;
                        sentNotification.TenantId = sent.TenantId;
                        sentNotification.TotalNumberOfSendThrottles = sent.TotalNumberOfSendThrottles;
                    }
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch(Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                throw;
            }

            return sentNotification;
        }
    }
}
