// <copyright file="TeamsRatingAnalyticsDataEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamsRatingAnalytics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Cosmos.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Rating data entity class.
    /// It holds the data for the content of the rating notification.
    /// It holds the data for the recipients of the rating notification.
    /// </summary>
    public class TeamsRatingAnalyticsDataEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the notification id.
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets numbers likes (Curtir) in message.
        /// </summary>
        public int IntLike { get; set; }

        /// <summary>
        /// Gets or sets numbers hearts (Coração) in message.
        /// </summary>
        public int IntHeart { get; set; }

        /// <summary>
        /// Gets or sets numbers Laugh (Gargalhada) in message.
        /// </summary>
        public int IntLaugh { get; set; }

        /// <summary>
        /// Gets or sets numbers surprise (Supreso) in message.
        /// </summary>
        public int IntSurprise { get; set; }

        /// <summary>
        /// Gets or sets numbers sad (Triste) in message.
        /// </summary>
        public int IntSad { get; set; }

        /// <summary>
        /// Gets or sets numbers angry (Bravo) in message.
        /// </summary>
        public int IntAngry { get; set; }

        /// <summary>
        /// Gets or sets Name Id Message.
        /// </summary>
        public string NmMessage { get; set; }
    }
}
