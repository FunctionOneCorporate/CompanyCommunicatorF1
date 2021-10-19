// <copyright file="TeamsRatingAnalytics.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.CompanyCommunicator.Models
{
    using System;

    /// <summary>
    /// Teams Rating Analytics model class.
    /// </summary>
    public class TeamsRatingAnalytics
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
        /// Gets or Sets Prymary Key.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or Sets Primary Key.
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or Sets save in storage.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        ///  Gets or sets Name the mensagem.
        /// </summary>
        public string NmMensagem { get; set; }
    }
}
