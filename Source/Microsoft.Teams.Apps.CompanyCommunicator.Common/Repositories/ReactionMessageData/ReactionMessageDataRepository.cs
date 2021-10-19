// <copyright file="ReactionMessageDataRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.ReactionMessageData
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Repository of the Reaction data in the table storage.
    /// </summary>
    public class ReactionMessageDataRepository : BaseRepository<ReactionMessageDataEntity>
    {
        /// <summary>Initializes a new instance of the <see cref="ReactionMessageDataRepository"/> class.
        /// </summary>
        /// <param name="logger">The logging service.</param>
        /// <param name="repositoryOptions">Options used to create the repository.</param>
        /// <param name="tableRowKeyGenerator">Table row key generator service.</param>
        public ReactionMessageDataRepository(
            ILogger<ReactionMessageDataRepository> logger,
            IOptions<RepositoryOptions> repositoryOptions,
            TableRowKeyGenerator tableRowKeyGenerator)
            : base(
                  logger,
                  storageAccountConnectionString: repositoryOptions.Value.StorageAccountConnectionString,
                  tableName: ReactionMessageDataTableNames.TableName,
                  defaultPartitionKey: ReactionMessageDataTableNames.ReactionMessagePartition,
                  ensureTableExists: repositoryOptions.Value.EnsureTableExists)
        {
            this.TableRowKeyGenerator = tableRowKeyGenerator;
        }

        /// <summary>
        /// Gets table row key generator.
        /// </summary>
        public TableRowKeyGenerator TableRowKeyGenerator { get; }

        /// <summary>
        /// This method ensures the ReactionMessageDataRepository table is created in the storage.
        /// This method should be called before kicking off an Azure function that uses the ReactionMessageDataRepository table.
        /// Otherwise the app will crash.
        /// By design, Azure functions (in this app) do not create a table if it's absent.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task EnsureReactionMessageDataTableExistsAsync()
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
        /// <param name="reactionMessageData">The draft notification instance to be moved to the sent partition.</param>
        /// <returns>The new DraftNotification ID.</returns>
        public async Task<string> CreateReactionData(ReactionMessageDataEntity reactionMessageData)
        {
            try
            {
                if (reactionMessageData == null)
                {
                    throw new ArgumentNullException(nameof(reactionMessageData));
                }

                var newReactionMessageDataId = this.TableRowKeyGenerator.CreateNewKeyOrderingMostRecentToOldest();

                // Create a sent notification based on the draft notification.
                var reactionMessageDataEntity = new ReactionMessageDataEntity
                {
                    PartitionKey = ReactionMessageDataTableNames.ReactionMessagePartition,
                    RowKey = newReactionMessageDataId,
                    MessageId = reactionMessageData.MessageId,
                    Reaction = reactionMessageData.Reaction,
                    AddReaction = reactionMessageData.AddReaction,
                    ChannelId = reactionMessageData.ChannelId,
                    ConversationId = reactionMessageData.ConversationId,
                    Tenant = reactionMessageData.Tenant,
                    IsGroup = reactionMessageData.IsGroup,
                    ConversationType = reactionMessageData.ConversationType,
                    AadObjectId = reactionMessageData.AadObjectId,
                    Email = reactionMessageData.Email,
                    GivenName = reactionMessageData.GivenName,
                    Name = reactionMessageData.Name,
                    Role = reactionMessageData.Role,
                    SurName = reactionMessageData.SurName,
                    UserPrincipalName = reactionMessageData.UserPrincipalName,
                    UserRole = reactionMessageData.UserRole,
                };
                await this.CreateOrUpdateAsync(reactionMessageDataEntity);

                return newReactionMessageDataId;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all Reaction Message entities from the table storage.
        /// </summary>
        /// <returns>All draft notification entities.</returns>
        public async Task<IEnumerable<ReactionMessageDataEntity>> GetAllReactionsMessageAsync()
        {
            var result = await this.GetAllAsync(ReactionMessageDataTableNames.ReactionMessagePartition);

            return result;
        }
    }
}
