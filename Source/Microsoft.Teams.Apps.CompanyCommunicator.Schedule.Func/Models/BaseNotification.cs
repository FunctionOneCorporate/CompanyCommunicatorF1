// <copyright file="BaseNotification.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Models
{
    using System;

    /// <summary>
    /// Base notification model class.
    /// </summary>
    public class BaseNotification
    {
        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets Title value.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Image Link value.
        /// </summary>
        public string ImageLink { get; set; }

        /// <summary>
        /// Gets or sets the Summary value.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the Author value.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the Button Title value.
        /// </summary>
        public string ButtonTitle { get; set; }

        /// <summary>
        /// Gets or sets the Button Link value.
        /// </summary>
        public string ButtonLink { get; set; }

        /// <summary>
        /// Gets or sets the Created DateTime value.
        /// </summary>
        public DateTime CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the notification is in the template.
        /// </summary>
        public bool Template { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the notification is in the Schedule.
        /// </summary>
        public bool Schedule { get; set; }

        /// <summary>
        /// Gets or sets the name the mensage.
        /// </summary>
        public string NmMensagem { get; set; }

        /// <summary>
        /// Gets or sets the headers image.
        /// </summary>
        public string HeaderImgLink { get; set; }

        /// <summary>
        /// Gets or sets the footers image.
        /// </summary>
        public string FooterImgLink { get; set; }

        /// <summary>
        ///  Gets or sets link of two button.
        /// </summary>
        public string ButtonLink2 { get; set; }

        /// <summary>
        /// Gets or sets Title of two button.
        /// </summary>
        public string ButtonTitle2 { get; set; }
    }
}
