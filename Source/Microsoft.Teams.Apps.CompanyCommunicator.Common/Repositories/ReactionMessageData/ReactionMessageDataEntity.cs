// <copyright file="ReactionMessageDataEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.ReactionMessageData
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Cosmos.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Reaction for users in message.
    /// </summary>
    public class ReactionMessageDataEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets id message in Microsoft Teams.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets reaction in message.
        /// </summary>
        public string Reaction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether add reaction or remove reaction.
        /// True is add Reaction, false remove reaction.
        /// </summary>
        public bool AddReaction { get; set; }

        /// <summary>
        /// Gets or sets id Channel conversation.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets id conversation.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets Tenant Id.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is group.
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// Gets or sets Type the conversation.
        /// </summary>
        public string ConversationType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether data processed.
        /// </summary>
        public bool IsProcessed { get; set; }

        /// <summary>
        /// Gets or sets id user.
        /// </summary>
        public string AadObjectId { get; set; }

        /// <summary>
        /// Gets or sets email user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets given name user.
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets name user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets role.
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets surname the user.
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Gets or sets principal name the user.
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets user role.
        /// </summary>
        public string UserRole { get; set; }
    }
}
