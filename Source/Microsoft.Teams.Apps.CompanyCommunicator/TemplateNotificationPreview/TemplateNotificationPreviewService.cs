﻿// <copyright file="TemplateNotificationPreviewService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.TemplateNotificationPreview
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.CompanyCommunicator.Bot;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.AdaptiveCard;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.CommonBot;

    /// <summary>
    /// Template notification preview service.
    /// </summary>
    public class TemplateNotificationPreviewService
    {
        private static readonly string MsTeamsChannelId = "msteams";
        private static readonly string ChannelConversationType = "channel";
        private static readonly string ThrottledErrorResponse = "Throttled";
        private readonly string botAppId;
        private readonly AdaptiveCardCreator adaptiveCardCreator;
        private readonly CompanyCommunicatorBotAdapter companyCommunicatorBotAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateNotificationPreviewService"/> class.
        /// </summary>
        /// <param name="botOptions">The bot options.</param>
        /// <param name="adaptiveCardCreator">Adaptive card creator service.</param>
        /// <param name="companyCommunicatorBotAdapter">Bot framework http adapter instance.</param>
        public TemplateNotificationPreviewService(
            IOptions<BotOptions> botOptions,
            AdaptiveCardCreator adaptiveCardCreator,
            CompanyCommunicatorBotAdapter companyCommunicatorBotAdapter)
        {
            this.botAppId = botOptions.Value.MicrosoftAppId;
            if (string.IsNullOrEmpty(this.botAppId))
            {
                throw new ApplicationException("MicrosoftAppId setting is missing in the configuration.");
            }

            this.adaptiveCardCreator = adaptiveCardCreator;
            this.companyCommunicatorBotAdapter = companyCommunicatorBotAdapter;
        }

        /// <summary>
        /// Send a preview of a draft notification.
        /// </summary>
        /// <param name="templateNotificationEntity">Draft notification entity.</param>
        /// <param name="teamDataEntity">The team data entity.</param>
        /// <param name="teamsChannelId">The Teams channel id.</param>
        /// <returns>It returns HttpStatusCode.OK, if this method triggers the bot service to send the adaptive card successfully.
        /// It returns HttpStatusCode.TooManyRequests, if the bot service throttled the request to send the adaptive card.</returns>
        public async Task<HttpStatusCode> SendPreview(NotificationDataEntity templateNotificationEntity, TeamDataEntity teamDataEntity, string teamsChannelId)
        {
            if (templateNotificationEntity == null)
            {
                throw new ArgumentException("Null draft notification entity.");
            }

            if (teamDataEntity == null)
            {
                throw new ArgumentException("Null team data entity.");
            }

            if (string.IsNullOrWhiteSpace(teamsChannelId))
            {
                throw new ArgumentException("Null channel id.");
            }

            // Create bot conversation reference.
            var conversationReference = this.PrepareConversationReferenceAsync(teamDataEntity, teamsChannelId);

            // Ensure the bot service URL is trusted.
            if (!MicrosoftAppCredentials.IsTrustedServiceUrl(conversationReference.ServiceUrl))
            {
                MicrosoftAppCredentials.TrustServiceUrl(conversationReference.ServiceUrl);
            }

            // Trigger bot to send the adaptive card.
            try
            {
                await this.companyCommunicatorBotAdapter.ContinueConversationAsync(
                    this.botAppId,
                    conversationReference,
                    async (turnContext, cancellationToken) => await this.SendAdaptiveCardAsync(turnContext, templateNotificationEntity),
                    CancellationToken.None);
                return HttpStatusCode.OK;
            }
            catch (ErrorResponseException e)
            {
                var errorResponse = (ErrorResponse)e.Body;
                if (errorResponse != null
                    && errorResponse.Error.Code.Equals(TemplateNotificationPreviewService.ThrottledErrorResponse, StringComparison.OrdinalIgnoreCase))
                {
                    return HttpStatusCode.TooManyRequests;
                }

                throw;
            }
        }

        private ConversationReference PrepareConversationReferenceAsync(TeamDataEntity teamDataEntity, string channelId)
        {
            var channelAccount = new ChannelAccount
            {
                Id = $"28:{this.botAppId}",
            };

            var conversationAccount = new ConversationAccount
            {
                ConversationType = TemplateNotificationPreviewService.ChannelConversationType,
                Id = channelId,
                TenantId = teamDataEntity.TenantId,
            };

            var conversationReference = new ConversationReference
            {
                Bot = channelAccount,
                ChannelId = TemplateNotificationPreviewService.MsTeamsChannelId,
                Conversation = conversationAccount,
                ServiceUrl = teamDataEntity.ServiceUrl,
            };

            return conversationReference;
        }

        private async Task SendAdaptiveCardAsync(
            ITurnContext turnContext,
            NotificationDataEntity templateNotificationEntity)
        {
            var reply = this.CreateReply(templateNotificationEntity);
            await turnContext.SendActivityAsync(reply);
        }

        private IMessageActivity CreateReply(NotificationDataEntity templateNotificationEntity)
        {
            var adaptiveCard = this.adaptiveCardCreator.CreateAdaptiveCard(
                templateNotificationEntity.Title,
                templateNotificationEntity.ImageLink,
                templateNotificationEntity.Summary,
                templateNotificationEntity.Author,
                templateNotificationEntity.ButtonTitle,
                templateNotificationEntity.ButtonLink,
                templateNotificationEntity.ButtonTitle2,
                templateNotificationEntity.ButtonLink2,
                templateNotificationEntity.HeaderImgLink,
                templateNotificationEntity.FooterImgLink);

            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard,
            };

            var reply = MessageFactory.Attachment(attachment);

            return reply;
        }
    }
}
