// <copyright file="ITeamsRatingService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models;

    /// <summary>
    /// Interface of methods for Teams Rating.
    /// </summary>
    public interface ITeamsRatingService
    {
        /// <summary>
        /// Find Rating Notification.
        /// </summary>
        /// <param name="notificationId"> Notification id. </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<TeamsRatingAnalytics> GetTeamsRatingForNotificationId(string notificationId);

        /// <summary>
        /// Set sumarize reactions in data table TeamsRating.
        /// </summary>
        /// <param name="teamsRatingAnalytics">Message find reaction.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task SetReactionsInTeamsRating(TeamsRatingAnalytics teamsRatingAnalytics);
    }
}
