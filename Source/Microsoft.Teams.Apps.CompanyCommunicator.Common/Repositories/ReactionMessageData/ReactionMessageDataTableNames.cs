// <copyright file="ReactionMessageDataTableNames.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.ReactionMessageData
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Reaction Message data table names.
    /// </summary>
    public class ReactionMessageDataTableNames
    {
        /// <summary>
        /// Table name for the notification data table.
        /// </summary>
        public static readonly string TableName = "ReactionMessageData";

        /// <summary>
        /// Analytics notifications partition key name.
        /// </summary>
        public static readonly string ReactionMessagePartition = "Reaction";
    }
}
