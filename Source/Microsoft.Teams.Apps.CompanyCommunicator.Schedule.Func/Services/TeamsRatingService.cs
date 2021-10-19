// <copyright file="TeamsRatingService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamsRatingAnalytics;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models;

    /// <summary>
    /// Class of implement methods for Teams Rating.
    /// </summary>
    public class TeamsRatingService : ITeamsRatingService
    {
        private readonly TeamsRatingAnalyticsDataRepository teamsRatingAnalyticsDataRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsRatingService"/> class.
        /// </summary>
        /// <param name="teamsRatingAnalyticsDataRepository">Then rating data repository.</param>
        public TeamsRatingService(
            TeamsRatingAnalyticsDataRepository teamsRatingAnalyticsDataRepository)
        {
            this.teamsRatingAnalyticsDataRepository = teamsRatingAnalyticsDataRepository ?? throw new ArgumentException(nameof(teamsRatingAnalyticsDataRepository));
        }

        /// <summary>
        /// Get Teams Rating for Notification id.
        /// </summary>
        /// <param name="notificationId"> Notification id. </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<TeamsRatingAnalytics> GetTeamsRatingForNotificationId(string notificationId)
        {
            TeamsRatingAnalytics teamsRatingAnalytics = new TeamsRatingAnalytics();

            try
            {
                string strParameters = $"PartitionKey eq 'Analytics' and RowKey eq '{notificationId}'";
                var teamsRatingAnalyticsEntities = await this.teamsRatingAnalyticsDataRepository.GetWithFilterAsync2(strParameters);

                if (teamsRatingAnalyticsEntities != null)
                {
                    foreach (var teams in teamsRatingAnalyticsEntities)
                    {
                        teamsRatingAnalytics.PartitionKey = teams.PartitionKey;
                        teamsRatingAnalytics.RowKey = teams.RowKey;
                        teamsRatingAnalytics.Timestamp = teams.Timestamp;
                        teamsRatingAnalytics.IntAngry = teams.IntAngry;
                        teamsRatingAnalytics.IntHeart = teams.IntHeart;
                        teamsRatingAnalytics.IntLaugh = teams.IntLaugh;
                        teamsRatingAnalytics.IntLike = teams.IntLike;
                        teamsRatingAnalytics.IntSad = teams.IntSurprise;
                        teamsRatingAnalytics.IntSurprise = teams.IntSurprise;
                        teamsRatingAnalytics.NotificationId = teams.NotificationId;
                        teamsRatingAnalytics.NmMensagem = teams.NmMessage;
                    }
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                throw;
            }

            return teamsRatingAnalytics;
        }

        /// <summary>
        /// Set sumarize reactions in data table TeamsRating.
        /// </summary>
        /// <param name="teamsRatingAnalytics">Message find reaction.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SetReactionsInTeamsRating(TeamsRatingAnalytics teamsRatingAnalytics)
        {
            try
            {
                if (teamsRatingAnalytics != null)
                {
                    var teamsRatingAnaliticsEntity = new TeamsRatingAnalyticsDataEntity
                    {
                        PartitionKey = teamsRatingAnalytics.PartitionKey,
                        RowKey = teamsRatingAnalytics.RowKey,
                        Timestamp = teamsRatingAnalytics.Timestamp.GetValueOrDefault(),
                        IntAngry = teamsRatingAnalytics.IntAngry,
                        IntHeart = teamsRatingAnalytics.IntHeart,
                        IntLaugh = teamsRatingAnalytics.IntLaugh,
                        IntLike = teamsRatingAnalytics.IntLike,
                        IntSad = teamsRatingAnalytics.IntSad,
                        IntSurprise = teamsRatingAnalytics.IntSurprise,
                        NotificationId = teamsRatingAnalytics.NotificationId,
                        NmMessage = teamsRatingAnalytics.NmMensagem,
                    };
                    await this.teamsRatingAnalyticsDataRepository.InsertOrMergeAsync(teamsRatingAnaliticsEntity);
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
