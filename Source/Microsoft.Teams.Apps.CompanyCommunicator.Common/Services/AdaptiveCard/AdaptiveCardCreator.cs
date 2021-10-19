// <copyright file="AdaptiveCardCreator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.AdaptiveCard
{
    using System;
    using System.Collections;
    using AdaptiveCards;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;

    /// <summary>
    /// Adaptive Card Creator service.
    /// </summary>
    public class AdaptiveCardCreator
    {
        /// <summary>
        /// Creates an adaptive card.
        /// </summary>
        /// <param name="notificationDataEntity">Notification data entity.</param>
        /// <returns>An adaptive card.</returns>
        public AdaptiveCard CreateAdaptiveCard(NotificationDataEntity notificationDataEntity)
        {
            return this.CreateAdaptiveCard(
                notificationDataEntity.Title,
                notificationDataEntity.ImageLink,
                notificationDataEntity.Summary,
                notificationDataEntity.Author,
                notificationDataEntity.ButtonTitle,
                notificationDataEntity.ButtonLink,
                notificationDataEntity.ButtonTitle2,
                notificationDataEntity.ButtonLink2,
                notificationDataEntity.HeaderImgLink,
                notificationDataEntity.FooterImgLink);
        }

        /// <summary>
        /// Create an adaptive card instance.
        /// </summary>
        /// <param name="title">The adaptive card's title value.</param>
        /// <param name="imageUrl">The adaptive card's image URL.</param>
        /// <param name="summary">The adaptive card's summary value.</param>
        /// <param name="author">The adaptive card's author value.</param>
        /// <param name="buttonTitle">The adaptive card's button title value.</param>
        /// <param name="buttonUrl">The adaptive card's button url value.</param>
        /// <param name="buttonTile2"> The title of secund button. </param>
        /// <param name="buttonUrl2"> The url the second button. </param>
        /// <param name="imgHeader"> The image of header card. </param>
        /// <param name="imgFooter"> The image of footer card. </param>
        /// <returns>The created adaptive card instance.</returns>
        public AdaptiveCard CreateAdaptiveCard(
            string title,
            string imageUrl,
            string summary,
            string author,
            string buttonTitle,
            string buttonUrl,
            string buttonTile2,
            string buttonUrl2,
            string imgHeader,
            string imgFooter)
        {
            var version = new AdaptiveSchemaVersion("1.2");
            AdaptiveCard card = new AdaptiveCard(version);

            // Image Header.
            if (!string.IsNullOrWhiteSpace(imgHeader))
            {
                card.Body.Add(new AdaptiveImage()
                {
                    Url = new Uri(imgHeader, UriKind.RelativeOrAbsolute),
                    Spacing = AdaptiveSpacing.Default,
                    Size = AdaptiveImageSize.Stretch,
                    AltText = string.Empty,
                });
            }

            // Title.
            if (!string.IsNullOrWhiteSpace(title))
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = title,
                    Size = AdaptiveTextSize.ExtraLarge,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true,
                });
            }

            // Summary.
            if (!string.IsNullOrWhiteSpace(summary))
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = summary,
                    Wrap = true,
                });
            }

            // Image principal.
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                card.Body.Add(new AdaptiveImage()
                {
                    Url = new Uri(imageUrl, UriKind.RelativeOrAbsolute),
                    Spacing = AdaptiveSpacing.Default,
                    Size = AdaptiveImageSize.Stretch,
                    AltText = string.Empty,
                });
            }

            // Author.
            if (!string.IsNullOrWhiteSpace(author))
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = author,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Lighter,
                    Wrap = true,
                });
            }

            // Image footer.
            if (!string.IsNullOrWhiteSpace(imgFooter))
            {
                card.Body.Add(new AdaptiveImage()
                {
                    Url = new Uri(imgFooter, UriKind.RelativeOrAbsolute),
                    Spacing = AdaptiveSpacing.Default,
                    Size = AdaptiveImageSize.Stretch,
                    AltText = string.Empty,
                });
            }

            // Image for log.
            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri("https://f1.com.br/log", UriKind.RelativeOrAbsolute),
                Spacing = AdaptiveSpacing.None,
                Size = AdaptiveImageSize.Small,
                Separator = false,
                AltText = string.Empty,
                Style = AdaptiveImageStyle.Person,
            });

            // Button 01
            if (!string.IsNullOrWhiteSpace(buttonTitle)
                && !string.IsNullOrWhiteSpace(buttonUrl))
            {
                card.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = buttonTitle,
                    Url = new Uri(buttonUrl, UriKind.RelativeOrAbsolute),
                });
            }

            // Button 02
            if (!string.IsNullOrWhiteSpace(buttonTile2)
                && !string.IsNullOrWhiteSpace(buttonUrl2))
            {
                card.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = buttonTile2,
                    Url = new Uri(buttonUrl2, UriKind.RelativeOrAbsolute),
                });
            }

            return card;
        }
    }
}
