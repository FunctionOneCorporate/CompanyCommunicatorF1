// <copyright file="ScheduleNotificationPreviewRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Models
{
    /// <summary>
    /// Schedule notification preview request model class.
    /// </summary>
    public class ScheduleNotificationPreviewRequest
    {
        /// <summary>
        /// Gets or sets Schedule notification id.
        /// </summary>
        public string ScheduleNotificationId { get; set; }

        /// <summary>
        /// Gets or sets Teams team id.
        /// </summary>
        public string TeamsTeamId { get; set; }

        /// <summary>
        /// Gets or sets Teams channel id.
        /// </summary>
        public string TeamsChannelId { get; set; }
    }
}
