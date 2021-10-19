// <copyright file="TeamsRatingAnalyticsDataRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamsRatingAnalytics
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Repository of the Rating data in the table storage.
    /// </summary>
    public class TeamsRatingAnalyticsDataRepository : BaseRepository<TeamsRatingAnalyticsDataEntity>
    {
        /// <summary>Initializes a new instance of the <see cref="TeamsRatingAnalyticsDataRepository"/> class.
        /// </summary>
        /// <param name="logger">The logging service.</param>
        /// <param name="repositoryOptions">Options used to create the repository.</param>
        /// <param name="tableRowKeyGenerator">Table row key generator service.</param>
        public TeamsRatingAnalyticsDataRepository(
            ILogger<TeamsRatingAnalyticsDataRepository> logger,
            IOptions<RepositoryOptions> repositoryOptions,
            TableRowKeyGenerator tableRowKeyGenerator)
            : base(
                  logger,
                  storageAccountConnectionString: repositoryOptions.Value.StorageAccountConnectionString,
                  tableName: TeamsRatingAnalyticsDataTableNames.TableName,
                  defaultPartitionKey: TeamsRatingAnalyticsDataTableNames.TeamsRatingAnalyticsPartition,
                  ensureTableExists: repositoryOptions.Value.EnsureTableExists)
        {
            this.TableRowKeyGenerator = tableRowKeyGenerator;
        }

        /// <summary>
        /// Gets table row key generator.
        /// </summary>
        public TableRowKeyGenerator TableRowKeyGenerator { get; }

        /// <summary>
        /// This method ensures the TeamsRatingAnalytics table is created in the storage.
        /// This method should be called before kicking off an Azure function that uses the TeamsRatingAnalytics table.
        /// Otherwise the app will crash.
        /// By design, Azure functions (in this app) do not create a table if it's absent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task EnsureTeamsRatingAnalyticsTableExistsAsync()
        {
            var exists = await this.Table.ExistsAsync();
            if (!exists)
            {
                await this.Table.CreateAsync();
            }
        }

        /// <summary>
        /// Create a new draft notification from template to sent partition.
        /// </summary>
        /// <param name="notificationId">The draft notification instance to be moved to the sent partition.</param>
        /// <param name="nameMessage"> The name identification message. </param>
        /// <returns>The new DraftNotification ID.</returns>
        public async Task<string> CreateTeamsRatingAnalyticsData(string notificationId, string nameMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(notificationId))
                {
                    throw new ArgumentNullException(nameof(notificationId));
                }

                // Create a sent notification based on the draft notification.
                var teams = new TeamsRatingAnalyticsDataEntity
                {
                    PartitionKey = TeamsRatingAnalyticsDataTableNames.TeamsRatingAnalyticsPartition,
                    RowKey = notificationId,
                    IntAngry = 0,
                    IntHeart = 0,
                    IntLaugh = 0,
                    IntLike = 0,
                    IntSad = 0,
                    IntSurprise = 0,
                    NotificationId = notificationId,
                    NmMessage = nameMessage,
                };
                await this.CreateOrUpdateAsync(teams);

                return notificationId;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
