// <copyright file="TeamsRatingAnalyticsDataTableNames.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamsRatingAnalytics
{
    /// <summary>
    /// Teams Rating data table names.
    /// </summary>
    public static class TeamsRatingAnalyticsDataTableNames
    {
        /// <summary>
        /// Table name for the notification data table.
        /// </summary>
        public static readonly string TableName = "TeamsRatingAnalytics";

        /// <summary>
        /// Analytics notifications partition key name.
        /// </summary>
        public static readonly string TeamsRatingAnalyticsPartition = "Analytics";
    }
}
