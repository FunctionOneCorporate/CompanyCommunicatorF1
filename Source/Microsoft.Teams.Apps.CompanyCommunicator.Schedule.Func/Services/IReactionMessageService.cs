// <copyright file="IReactionMessageService.cs" company="Microsoft">
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
    /// Interface of methods for Reaction Message.
    /// </summary>
    public interface IReactionMessageService
    {
        /// <summary>
        /// Find Rating Notification.
        /// </summary>
        /// <param name="proced"> Notification id. </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task<List<ReactionMessageData>> GetReactionMessageForIsProced(bool proced);

        /// <summary>
        /// Set sumarize reactions in data table TeamsRating.
        /// </summary>
        /// <param name="reactionMessageData">Message find reaction.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task SetReactionsMessageProced(ReactionMessageData reactionMessageData);
    }
}
