// <copyright file="FindTeamsRating.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Services;

    /// <summary>
    /// Service of find rating messages.
    /// </summary>
    public class FindTeamsRating
    {
        private readonly ITeamsRatingService teamsRatingService;
        private readonly IReactionMessageService reactionMessageService;
        private readonly INotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindTeamsRating"/> class.
        /// </summary>
        /// <param name="teamsRatingService"> Class the manipulation teams rating data.</param>
        /// <param name="reactionMessageService"> Class the manipulation reaction data.</param>
        /// <param name="notificationService"> Class the manipulation notification data.</param>
        public FindTeamsRating(ITeamsRatingService teamsRatingService, IReactionMessageService reactionMessageService, INotificationService notificationService)
        {
            this.teamsRatingService = teamsRatingService ?? throw new ArgumentException(nameof(teamsRatingService));
            this.reactionMessageService = reactionMessageService ?? throw new ArgumentException(nameof(reactionMessageService));
            this.notificationService = notificationService ?? throw new ArgumentException(nameof(notificationService));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunAsync"/> class.
        /// </summary>
        /// <param name="myTimer">Timer for precessing.</param>
        /// <param name="log">Log operations.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName("FindTeamsRating")]
        public async Task RunAsync([TimerTrigger("0 0 */8 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                // variaveis
                bool blProced = false;
                var reactionList = new List<ReactionMessageData>();

                // find reaction for summarization.
                reactionList = await this.reactionMessageService.GetReactionMessageForIsProced(blProced);

                if (reactionList != null)
                {
                    if (reactionList.Count > 0)
                    {
                        foreach (var react in reactionList)
                        {
                            // find sent notification for get notification id
                            var sentNotification = await this.notificationService.GetSentNotificationforMessageIdTeams(react.MessageId, react.ConversationId);

                            if (sentNotification != null)
                            {
                                // find teams rating for notification id, for update rating.
                                var teamsRating = await this.teamsRatingService.GetTeamsRatingForNotificationId(sentNotification.PartitionKey);

                                if (teamsRating != null)
                                {
                                    if (react.AddReaction)
                                    {
                                        // add Reaction
                                        switch (react.Reaction)
                                        {
                                            case "like":
                                                teamsRating.IntLike += 1;
                                                break;
                                            case "heart":
                                                teamsRating.IntHeart += 1;
                                                break;
                                            case "laugh":
                                                teamsRating.IntLaugh += 1;
                                                break;
                                            case "surprised":
                                                teamsRating.IntSurprise += 1;
                                                break;
                                            case "sad":
                                                teamsRating.IntSad += 1;
                                                break;
                                            default:
                                                teamsRating.IntAngry += 1;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        // retire Reaction
                                        switch (react.Reaction)
                                        {
                                            case "like":
                                                if (teamsRating.IntLike > 0)
                                                {
                                                    teamsRating.IntLike -= 1;
                                                }

                                                break;
                                            case "heart":
                                                if (teamsRating.IntHeart > 0)
                                                {
                                                    teamsRating.IntHeart -= 1;
                                                }

                                                break;
                                            case "laugh":
                                                if (teamsRating.IntLaugh > 0)
                                                {
                                                    teamsRating.IntLaugh -= 1;
                                                }

                                                break;
                                            case "surprised":
                                                if (teamsRating.IntSurprise > 0)
                                                {
                                                    teamsRating.IntSurprise -= 1;
                                                }

                                                break;
                                            case "sad":
                                                if (teamsRating.IntSad > 0)
                                                {
                                                    teamsRating.IntSad -= 1;
                                                }

                                                break;
                                            default:
                                                if (teamsRating.IntAngry > 0)
                                                {
                                                    teamsRating.IntAngry -= 1;
                                                }

                                                break;
                                        }
                                    }

                                    // Update team rating
                                    await this.teamsRatingService.SetReactionsInTeamsRating(teamsRating);

                                    // Update reaction data
                                    react.IsProcessed = true;
                                    await this.reactionMessageService.SetReactionsMessageProced(react);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
            }

            log.LogInformation($"End C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
