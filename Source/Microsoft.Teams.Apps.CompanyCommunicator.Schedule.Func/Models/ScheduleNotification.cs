// <copyright file="ScheduleNotification.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Resources;

    /// <summary>
    /// Schedule notification model class.
    /// </summary>
    public class ScheduleNotification : BaseNotification
    {
        private static readonly int MaxSelectedTeamNum = 20;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleNotification"/> class.
        /// </summary>
        public ScheduleNotification()
        {
            this.Teams = new List<string>();
            this.Rosters = new List<string>();
        }

        /// <summary>
        /// Gets or sets the Created DateTime value.
        /// </summary>
        public string ScheduleDate { get; set; }

        /// <summary>
        /// Gets or sets Teams audience id collection.
        /// </summary>
        public IEnumerable<string> Teams { get; set; }

        /// <summary>
        /// Gets or sets Rosters audience id collection.
        /// </summary>
        public IEnumerable<string> Rosters { get; set; }

        /// <summary>
        /// Gets or sets Groups audience id collection.
        /// </summary>
        public IEnumerable<string> Groups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a notification should be sent to all the users.
        /// </summary>
        public bool AllUsers { get; set; }

        /// <summary>
        /// Validates a draft notification.
        /// Teams and Rosters property should not contain more than 20 items.
        /// </summary>
        /// <param name="localizer">The string localizer service.</param>
        /// <param name="errorMessage">It returns the error message found by the method to the callers.</param>
        /// <returns>A flag indicates if a draft notification is valid or not.</returns>
        public bool Validate(IStringLocalizer<Strings> localizer, out string errorMessage)
        {
            var stringBuilder = new StringBuilder();

            var teams = this.Teams.ToList();
            if (teams.Count > ScheduleNotification.MaxSelectedTeamNum)
            {
                var format = localizer.GetString("NumberOfTeamsExceededLimitWarningFormat");
                stringBuilder.AppendFormat(format, teams.Count, ScheduleNotification.MaxSelectedTeamNum);
                stringBuilder.AppendLine();
            }

            var rosters = this.Rosters.ToList();
            if (rosters.Count > ScheduleNotification.MaxSelectedTeamNum)
            {
                var format = localizer.GetString("NumberOfRostersExceededLimitWarningFormat");
                stringBuilder.AppendFormat(format, rosters.Count, ScheduleNotification.MaxSelectedTeamNum);
                stringBuilder.AppendLine();
            }

            var groups = this.Groups.ToList();
            if (groups.Count > ScheduleNotification.MaxSelectedTeamNum)
            {
                var format = localizer.GetString("NumberOfGroupsExceededLimitWarningFormat");
                stringBuilder.AppendFormat(format, groups.Count, ScheduleNotification.MaxSelectedTeamNum);
                stringBuilder.AppendLine();
            }

            errorMessage = stringBuilder.ToString();
            return stringBuilder.Length == 0;
        }
    }
}
