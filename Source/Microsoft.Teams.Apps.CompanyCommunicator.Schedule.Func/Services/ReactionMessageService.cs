// <copyright file="ReactionMessageService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.ReactionMessageData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models;

    /// <summary>
    /// Class of implement methods for Reaction Message.
    /// </summary>
    public class ReactionMessageService : IReactionMessageService
    {
        private readonly ReactionMessageDataRepository reactionMessageDataRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactionMessageService"/> class.
        /// </summary>
        /// <param name="reactionMessageDataRepository">Then rating data repository.</param>
        public ReactionMessageService(
            ReactionMessageDataRepository reactionMessageDataRepository)
        {
            this.reactionMessageDataRepository = reactionMessageDataRepository ?? throw new ArgumentException(nameof(reactionMessageDataRepository));
        }

        /// <summary>
        /// Find Rating Notification.
        /// </summary>
        /// <param name="proced"> Notification id. </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<ReactionMessageData>> GetReactionMessageForIsProced(bool proced)
        {
            var reactionMessageList = new List<ReactionMessageData>();

            try
            {
                string strParameters = $"IsProcessed eq {proced}";
                //var reactionMessageDataEntities = await this.reactionMessageDataRepository.GetWithFilterAsync(strParameters, ReactionMessageDataTableNames.ReactionMessagePartition);
                var reactionMessageDataEntities = await this.reactionMessageDataRepository.GetAllReactionsMessageAsync();

                if (reactionMessageDataEntities != null)
                {
                    foreach (var reaction in reactionMessageDataEntities)
                    {
                        if (reaction.IsProcessed == false)
                        {
                            var reactionMessage = new ReactionMessageData
                            {
                                PartitionKey = reaction.PartitionKey,
                                RowKey = reaction.RowKey,
                                Timestamp = reaction.Timestamp,
                                AadObjectId = reaction.AadObjectId,
                                AddReaction = reaction.AddReaction,
                                ChannelId = reaction.ChannelId,
                                ConversationId = reaction.ConversationId,
                                ConversationType = reaction.ConversationType,
                                Email = reaction.Email,
                                GivenName = reaction.GivenName,
                                IsGroup = reaction.IsGroup,
                                IsProcessed = reaction.IsProcessed,
                                MessageId = reaction.MessageId,
                                Name = reaction.Name,
                            };

                            reactionMessageList.Add(reactionMessage);
                        }
                    }
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                throw;
            }

            return reactionMessageList;
        }

        /// <summary>
        /// Set sumarize reactions in data table TeamsRating.
        /// </summary>
        /// <param name="reactionMessageData">Message find reaction.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetReactionsMessageProced(ReactionMessageData reactionMessageData)
        {
            try
            {
                if (reactionMessageData != null)
                {
                    var reactionMessageDataEntity = new ReactionMessageDataEntity
                    {
                        PartitionKey = reactionMessageData.PartitionKey,
                        RowKey = reactionMessageData.RowKey,
                        Timestamp = reactionMessageData.Timestamp.GetValueOrDefault(),
                        AadObjectId = reactionMessageData.AadObjectId,
                        AddReaction = reactionMessageData.AddReaction,
                        ChannelId = reactionMessageData.ChannelId,
                        ConversationId = reactionMessageData.ConversationId,
                        ConversationType = reactionMessageData.ConversationType,
                        Email = reactionMessageData.Email,
                        GivenName = reactionMessageData.GivenName,
                        IsGroup = reactionMessageData.IsGroup,
                        IsProcessed = reactionMessageData.IsProcessed,
                        MessageId = reactionMessageData.MessageId,
                        Name = reactionMessageData.Name,
                    };
                    await this.reactionMessageDataRepository.InsertOrMergeAsync(reactionMessageDataEntity);
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                throw;
            }
        }
    }
}
