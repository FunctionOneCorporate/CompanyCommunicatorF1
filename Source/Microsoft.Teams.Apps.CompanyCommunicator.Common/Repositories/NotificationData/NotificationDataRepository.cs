// <copyright file="NotificationDataRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Repository of the notification data in the table storage.
    /// </summary>
    public class NotificationDataRepository : BaseRepository<NotificationDataEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationDataRepository"/> class.
        /// </summary>
        /// <param name="logger">The logging service.</param>
        /// <param name="repositoryOptions">Options used to create the repository.</param>
        /// <param name="tableRowKeyGenerator">Table row key generator service.</param>
        public NotificationDataRepository(
            ILogger<NotificationDataRepository> logger,
            IOptions<RepositoryOptions> repositoryOptions,
            TableRowKeyGenerator tableRowKeyGenerator)
            : base(
                  logger,
                  storageAccountConnectionString: repositoryOptions.Value.StorageAccountConnectionString,
                  tableName: NotificationDataTableNames.TableName,
                  defaultPartitionKey: NotificationDataTableNames.DraftNotificationsPartition,
                  ensureTableExists: repositoryOptions.Value.EnsureTableExists)
        {
            this.TableRowKeyGenerator = tableRowKeyGenerator;
        }

        /// <summary>
        /// Gets table row key generator.
        /// </summary>
        public TableRowKeyGenerator TableRowKeyGenerator { get; }

        /// <summary>
        /// Get all draft notification entities from the table storage.
        /// </summary>
        /// <returns>All draft notification entities.</returns>
        public async Task<IEnumerable<NotificationDataEntity>> GetAllDraftNotificationsAsync()
        {
            var result = await this.GetAllAsync(NotificationDataTableNames.DraftNotificationsPartition);

            return result;
        }

        /// <summary>
        /// Get the top 25 most recently sent notification entities from the table storage.
        /// </summary>
        /// <returns>The top 25 most recently sent notification entities.</returns>
        public async Task<IEnumerable<NotificationDataEntity>> GetMostRecentSentNotificationsAsync()
        {
            var result = await this.GetAllAsync(NotificationDataTableNames.SentNotificationsPartition, 25);

            return result;
        }

        /// <summary>
        /// Move a draft notification from draft to sent partition.
        /// </summary>
        /// <param name="draftNotificationEntity">The draft notification instance to be moved to the sent partition.</param>
        /// <returns>The new SentNotification ID.</returns>
        public async Task<string> MoveDraftToSentPartitionAsync(NotificationDataEntity draftNotificationEntity)
        {
            try
            {
                if (draftNotificationEntity == null)
                {
                    throw new ArgumentNullException(nameof(draftNotificationEntity));
                }

                var newSentNotificationId = this.TableRowKeyGenerator.CreateNewKeyOrderingMostRecentToOldest();

                // Create a sent notification based on the draft notification.
                var sentNotificationEntity = new NotificationDataEntity
                {
                    PartitionKey = NotificationDataTableNames.SentNotificationsPartition,
                    RowKey = newSentNotificationId,
                    Id = newSentNotificationId,
                    Title = draftNotificationEntity.Title,
                    ImageLink = draftNotificationEntity.ImageLink,
                    Summary = draftNotificationEntity.Summary,
                    Author = draftNotificationEntity.Author,
                    ButtonTitle = draftNotificationEntity.ButtonTitle,
                    ButtonLink = draftNotificationEntity.ButtonLink,
                    CreatedBy = draftNotificationEntity.CreatedBy,
                    CreatedDate = draftNotificationEntity.CreatedDate,
                    SentDate = null,
                    IsDraft = false,
                    Teams = draftNotificationEntity.Teams,
                    Rosters = draftNotificationEntity.Rosters,
                    Groups = draftNotificationEntity.Groups,
                    AllUsers = draftNotificationEntity.AllUsers,
                    MessageVersion = draftNotificationEntity.MessageVersion,
                    Succeeded = 0,
                    Failed = 0,
                    Throttled = 0,
                    TotalMessageCount = draftNotificationEntity.TotalMessageCount,
                    SendingStartedDate = DateTime.UtcNow,
                    Status = NotificationStatus.Queued.ToString(),
                    NmMensagem = draftNotificationEntity.NmMensagem,
                    HeaderImgLink = draftNotificationEntity.HeaderImgLink,
                    FooterImgLink = draftNotificationEntity.FooterImgLink,
                    ButtonLink2 = draftNotificationEntity.ButtonLink2,
                    ButtonTitle2 = draftNotificationEntity.ButtonTitle2,
                };
                await this.CreateOrUpdateAsync(sentNotificationEntity);

                // Delete the draft notification.
                await this.DeleteAsync(draftNotificationEntity);

                return newSentNotificationId;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Duplicate an existing draft notification.
        /// </summary>
        /// <param name="notificationEntity">The notification entity to be duplicated.</param>
        /// <param name="createdBy">Created by.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task DuplicateDraftNotificationAsync(
            NotificationDataEntity notificationEntity,
            string createdBy)
        {
            try
            {
                var newId = this.TableRowKeyGenerator.CreateNewKeyOrderingOldestToMostRecent();

                // TODO: Set the string "(copy)" in a resource file for multi-language support.
                var newNotificationEntity = new NotificationDataEntity
                {
                    PartitionKey = NotificationDataTableNames.DraftNotificationsPartition,
                    RowKey = newId,
                    Id = newId,
                    Title = notificationEntity.Title,
                    ImageLink = notificationEntity.ImageLink,
                    Summary = notificationEntity.Summary,
                    Author = notificationEntity.Author,
                    ButtonTitle = notificationEntity.ButtonTitle,
                    ButtonLink = notificationEntity.ButtonLink,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow,
                    IsDraft = true,
                    Teams = notificationEntity.Teams,
                    Groups = notificationEntity.Groups,
                    Rosters = notificationEntity.Rosters,
                    AllUsers = notificationEntity.AllUsers,
                    NmMensagem = notificationEntity.NmMensagem,
                    HeaderImgLink = notificationEntity.HeaderImgLink,
                    FooterImgLink = notificationEntity.FooterImgLink,
                    ButtonLink2 = notificationEntity.ButtonLink2,
                    ButtonTitle2 = notificationEntity.ButtonTitle2,
                };

                await this.CreateOrUpdateAsync(newNotificationEntity);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Updates notification status.
        /// </summary>
        /// <param name="notificationId">Notificaion Id.</param>
        /// <param name="status">Status.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateNotificationStatusAsync(string notificationId, NotificationStatus status)
        {
            var notificationDataEntity = await this.GetAsync(
                NotificationDataTableNames.SentNotificationsPartition,
                notificationId);

            if (notificationDataEntity != null)
            {
                notificationDataEntity.Status = status.ToString();
                await this.CreateOrUpdateAsync(notificationDataEntity);
            }
        }

        /// <summary>
        /// Save exception error message in a notification data entity.
        /// </summary>
        /// <param name="notificationDataEntityId">Notification data entity id.</param>
        /// <param name="errorMessage">Error message.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SaveExceptionInNotificationDataEntityAsync(
            string notificationDataEntityId,
            string errorMessage)
        {
            var notificationDataEntity = await this.GetAsync(
                NotificationDataTableNames.SentNotificationsPartition,
                notificationDataEntityId);
            if (notificationDataEntity != null)
            {
                notificationDataEntity.ErrorMessage =
                    this.AppendNewLine(notificationDataEntity.ErrorMessage, errorMessage);
                notificationDataEntity.Status = NotificationStatus.Failed.ToString();

                // Set the end date as current date.
                notificationDataEntity.SentDate = DateTime.UtcNow;

                await this.CreateOrUpdateAsync(notificationDataEntity);
            }
        }

        /// <summary>
        /// Save warning message in a notification data entity.
        /// </summary>
        /// <param name="notificationDataEntityId">Notification data entity id.</param>
        /// <param name="warningMessage">Warning message to be saved.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SaveWarningInNotificationDataEntityAsync(
            string notificationDataEntityId,
            string warningMessage)
        {
            try
            {
                var notificationDataEntity = await this.GetAsync(
                    NotificationDataTableNames.SentNotificationsPartition,
                    notificationDataEntityId);
                if (notificationDataEntity != null)
                {
                    notificationDataEntity.WarningMessage =
                        this.AppendNewLine(notificationDataEntity.WarningMessage, warningMessage);
                    await this.CreateOrUpdateAsync(notificationDataEntity);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Duplicate an existing schedule notification.
        /// </summary>
        /// <param name="notificationEntity">The notification entity to be duplicated.</param>
        /// <param name="createdBy">Created by.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task DuplicateScheduleNotificationAsync(
            NotificationDataEntity notificationEntity,
            string createdBy)
        {
            try
            {
                var newId = this.TableRowKeyGenerator.CreateNewKeyOrderingOldestToMostRecent();

                // TODO: Set the string "(copy)" in a resource file for multi-language support.
                var newNotificationEntity = new NotificationDataEntity
                {
                    PartitionKey = NotificationDataTableNames.ScheduleNotificationsPartition,
                    RowKey = newId,
                    Id = newId,
                    Title = notificationEntity.Title,
                    ImageLink = notificationEntity.ImageLink,
                    Summary = notificationEntity.Summary,
                    Author = notificationEntity.Author,
                    ButtonTitle = notificationEntity.ButtonTitle,
                    ButtonLink = notificationEntity.ButtonLink,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow,
                    IsDraft = true,
                    Teams = notificationEntity.Teams,
                    Groups = notificationEntity.Groups,
                    Rosters = notificationEntity.Rosters,
                    AllUsers = notificationEntity.AllUsers,
                    Template = notificationEntity.Template,
                    Schedule = notificationEntity.Schedule,
                    ScheduleDate = notificationEntity.ScheduleDate,
                    NmMensagem = notificationEntity.NmMensagem,
                    HeaderImgLink = notificationEntity.HeaderImgLink,
                    FooterImgLink = notificationEntity.FooterImgLink,
                    ButtonLink2 = notificationEntity.ButtonLink2,
                    ButtonTitle2 = notificationEntity.ButtonTitle2,
                };

                await this.CreateOrUpdateAsync(newNotificationEntity);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Duplicate an existing schedule notification.
        /// </summary>
        /// <param name="notificationEntity">The notification entity to be duplicated.</param>
        /// <param name="createdBy">Created by.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task DuplicateTemplateNotificationAsync(
            NotificationDataEntity notificationEntity,
            string createdBy)
        {
            try
            {
                var newId = this.TableRowKeyGenerator.CreateNewKeyOrderingOldestToMostRecent();

                // TODO: Set the string "(copy)" in a resource file for multi-language support.
                var newNotificationEntity = new NotificationDataEntity
                {
                    PartitionKey = NotificationDataTableNames.TemplateNotoficationsPartition,
                    RowKey = newId,
                    Id = newId,
                    Title = notificationEntity.Title,
                    ImageLink = notificationEntity.ImageLink,
                    Summary = notificationEntity.Summary,
                    Author = notificationEntity.Author,
                    ButtonTitle = notificationEntity.ButtonTitle,
                    ButtonLink = notificationEntity.ButtonLink,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow,
                    IsDraft = false,
                    Teams = notificationEntity.Teams,
                    Groups = notificationEntity.Groups,
                    Rosters = notificationEntity.Rosters,
                    AllUsers = notificationEntity.AllUsers,
                    Template = notificationEntity.Template,
                    Schedule = false,
                    NmMensagem = notificationEntity.NmMensagem,
                    HeaderImgLink = notificationEntity.HeaderImgLink,
                    FooterImgLink = notificationEntity.FooterImgLink,
                    ButtonLink2 = notificationEntity.ButtonLink2,
                    ButtonTitle2 = notificationEntity.ButtonTitle2,
                };

                await this.CreateOrUpdateAsync(newNotificationEntity);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all schedule notification entities from the table storage.
        /// </summary>
        /// <returns>All Schedule notification entities.</returns>
        public async Task<IEnumerable<NotificationDataEntity>> GetAllScheduleNotificationsAsync()
        {
            var result = await this.GetAllAsync(NotificationDataTableNames.ScheduleNotificationsPartition);

            return result;
        }

        /// <summary>
        /// Move a draft notification from draft to sent partition.
        /// </summary>
        /// <param name="scheduleNotificationEntity">The draft notification instance to be moved to the sent partition.</param>
        /// <returns>The new SentNotification ID.</returns>
        public async Task<string> MoveScheduleToSentPartitionAsync(NotificationDataEntity scheduleNotificationEntity)
        {
            try
            {
                if (scheduleNotificationEntity == null)
                {
                    throw new ArgumentNullException(nameof(scheduleNotificationEntity));
                }

                var newSentNotificationId = this.TableRowKeyGenerator.CreateNewKeyOrderingMostRecentToOldest();

                // Create a sent notification based on the draft notification.
                var sentNotificationEntity = new NotificationDataEntity
                {
                    PartitionKey = NotificationDataTableNames.SentNotificationsPartition,
                    RowKey = newSentNotificationId,
                    Id = newSentNotificationId,
                    Title = scheduleNotificationEntity.Title,
                    ImageLink = scheduleNotificationEntity.ImageLink,
                    Summary = scheduleNotificationEntity.Summary,
                    Author = scheduleNotificationEntity.Author,
                    ButtonTitle = scheduleNotificationEntity.ButtonTitle,
                    ButtonLink = scheduleNotificationEntity.ButtonLink,
                    CreatedBy = scheduleNotificationEntity.CreatedBy,
                    CreatedDate = scheduleNotificationEntity.CreatedDate,
                    SentDate = null,
                    IsDraft = false,
                    Teams = scheduleNotificationEntity.Teams,
                    Rosters = scheduleNotificationEntity.Rosters,
                    Groups = scheduleNotificationEntity.Groups,
                    AllUsers = scheduleNotificationEntity.AllUsers,
                    MessageVersion = scheduleNotificationEntity.MessageVersion,
                    Succeeded = 0,
                    Failed = 0,
                    Throttled = 0,
                    TotalMessageCount = scheduleNotificationEntity.TotalMessageCount,
                    SendingStartedDate = DateTime.UtcNow,
                    Status = NotificationStatus.Queued.ToString(),
                    Template = scheduleNotificationEntity.Template,
                    Schedule = scheduleNotificationEntity.Schedule,
                    ScheduleDate = scheduleNotificationEntity.ScheduleDate,
                    NmMensagem = scheduleNotificationEntity.NmMensagem,
                    HeaderImgLink = scheduleNotificationEntity.HeaderImgLink,
                    FooterImgLink = scheduleNotificationEntity.FooterImgLink,
                    ButtonLink2 = scheduleNotificationEntity.ButtonLink2,
                    ButtonTitle2 = scheduleNotificationEntity.ButtonTitle2,
                };
                await this.CreateOrUpdateAsync(sentNotificationEntity);

                // Delete the draft notification.
                await this.DeleteAsync(scheduleNotificationEntity);

                return newSentNotificationId;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all template notification entities from the table storage.
        /// </summary>
        /// <returns>All template notification entities.</returns>
        public async Task<IEnumerable<NotificationDataEntity>> GetAllTemplateNotificationsAsync()
        {
            var result = await this.GetAllAsync(NotificationDataTableNames.TemplateNotoficationsPartition);

            return result;
        }

        /// <summary>
        /// Create a new draft notification from template to sent partition.
        /// </summary>
        /// <param name="templateNotificationEntity">The draft notification instance to be moved to the sent partition.</param>
        /// <returns>The new DraftNotification ID.</returns>
        public async Task<string> CreateNewDrafttoTemplatePartition(NotificationDataEntity templateNotificationEntity)
        {
            try
            {
                if (templateNotificationEntity == null)
                {
                    throw new ArgumentNullException(nameof(templateNotificationEntity));
                }

                var newDraftNotificationId = this.TableRowKeyGenerator.CreateNewKeyOrderingMostRecentToOldest();

                // Create a sent notification based on the draft notification.
                var draftNotificationEntity = new NotificationDataEntity
                {
                    PartitionKey = NotificationDataTableNames.DraftNotificationsPartition,
                    RowKey = newDraftNotificationId,
                    Id = newDraftNotificationId,
                    Title = templateNotificationEntity.Title,
                    ImageLink = templateNotificationEntity.ImageLink,
                    Summary = templateNotificationEntity.Summary,
                    Author = templateNotificationEntity.Author,
                    ButtonTitle = templateNotificationEntity.ButtonTitle,
                    ButtonLink = templateNotificationEntity.ButtonLink,
                    CreatedBy = templateNotificationEntity.CreatedBy,
                    CreatedDate = templateNotificationEntity.CreatedDate,
                    SentDate = null,
                    IsDraft = false,
                    Teams = templateNotificationEntity.Teams,
                    Rosters = templateNotificationEntity.Rosters,
                    Groups = templateNotificationEntity.Groups,
                    AllUsers = templateNotificationEntity.AllUsers,
                    MessageVersion = templateNotificationEntity.MessageVersion,
                    Succeeded = 0,
                    Failed = 0,
                    Throttled = 0,
                    TotalMessageCount = templateNotificationEntity.TotalMessageCount,
                    SendingStartedDate = null,
                    Status = null,
                    Template = false,
                    Schedule = false,
                    ScheduleDate = null,
                    NmMensagem = templateNotificationEntity.NmMensagem,
                    HeaderImgLink = templateNotificationEntity.HeaderImgLink,
                    FooterImgLink = templateNotificationEntity.FooterImgLink,
                    ButtonLink2 = templateNotificationEntity.ButtonLink2,
                    ButtonTitle2 = templateNotificationEntity.ButtonTitle2,
                };
                await this.CreateOrUpdateAsync(draftNotificationEntity);

                return newDraftNotificationId;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private string AppendNewLine(string originalString, string newString)
        {
            return string.IsNullOrWhiteSpace(originalString)
                ? newString
                : $"{originalString}{Environment.NewLine}{newString}";
        }
    }
}
