// <copyright file="ScheduleNotificationSummary.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Models
{
    using System;

    /// <summary>
    /// Schedule Notification Summary model class.
    /// </summary>
    public class ScheduleNotificationSummary
    {
        /// <summary>
        /// Gets or sets Notification Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Title value.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Schedule DateTime value.
        /// </summary>
        public string ScheduleDate { get; set; }

        /// <summary>
        /// Gets or sets name mensagem.
        /// </summary>
        public string NmMensagem { get; set;  }
    }
}