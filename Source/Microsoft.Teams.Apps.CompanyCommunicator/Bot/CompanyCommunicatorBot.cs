// <copyright file="CompanyCommunicatorBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.ReactionMessageData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Resources;
    using Microsoft.Teams.Apps.CompanyCommunicator.Models;
    using Microsoft.Teams.Apps.CompanyCommunicator.Repositories.Extensions;

    /// <summary>
    /// Company Communicator Bot.
    /// Captures user data, team data, upload files.
    /// </summary>
    public class CompanyCommunicatorBot : TeamsActivityHandler
    {
        private static readonly string TeamRenamedEventType = "teamRenamed";

        private readonly TeamsDataCapture teamsDataCapture;
        private readonly TeamsFileUpload teamsFileUpload;
        private readonly IStringLocalizer<Strings> localizer;
        private readonly ReactionMessageDataRepository reactionMessageDataRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompanyCommunicatorBot"/> class.
        /// </summary>
        /// <param name="teamsDataCapture">Teams data capture service.</param>
        /// <param name="teamsFileUpload">change this.</param>
        /// <param name="localizer">Localization service.</param>
        /// <param name="reactionMessageDataRepository">Insert rating.</param>
        public CompanyCommunicatorBot(
            TeamsDataCapture teamsDataCapture,
            TeamsFileUpload teamsFileUpload,
            IStringLocalizer<Strings> localizer,
            ReactionMessageDataRepository reactionMessageDataRepository)
        {
            this.teamsDataCapture = teamsDataCapture ?? throw new ArgumentNullException(nameof(teamsDataCapture));
            this.teamsFileUpload = teamsFileUpload ?? throw new ArgumentNullException(nameof(teamsFileUpload));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.reactionMessageDataRepository = reactionMessageDataRepository ?? throw new ArgumentException(nameof(reactionMessageDataRepository));
        }

        /// <summary>
        /// Invoked when a conversation update activity is received from the channel.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // base.OnConversationUpdateActivityAsync is useful when it comes to responding to users being added to or removed from the conversation.
            // For example, a bot could respond to a user being added by greeting the user.
            // By default, base.OnConversationUpdateActivityAsync will call <see cref="OnMembersAddedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
            // if any users have been added or <see cref="OnMembersRemovedAsync(IList{ChannelAccount}, ITurnContext{IConversationUpdateActivity}, CancellationToken)"/>
            // if any users have been removed. base.OnConversationUpdateActivityAsync checks the member ID so that it only responds to updates regarding members other than the bot itself.
            await base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);

            var activity = turnContext.Activity;

            var isTeamRenamed = this.IsTeamInformationUpdated(activity);
            if (isTeamRenamed)
            {
                await this.teamsDataCapture.OnTeamInformationUpdatedAsync(activity);
            }

            if (activity.MembersAdded != null)
            {
                await this.teamsDataCapture.OnBotAddedAsync(activity);
            }

            if (activity.MembersRemoved != null)
            {
                await this.teamsDataCapture.OnBotRemovedAsync(activity);
            }
        }

        /// <summary>
        /// Invoke when a file upload accept consent activitiy is received from the channel.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="fileConsentCardResponse">The accepted response object of File Card.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task reprsenting asynchronous operation.</returns>
        protected override async Task OnTeamsFileConsentAcceptAsync(
            ITurnContext<IInvokeActivity> turnContext,
            FileConsentCardResponse fileConsentCardResponse,
            CancellationToken cancellationToken)
        {
            var (fileName, notificationId) = this.teamsFileUpload.ExtractInformation(fileConsentCardResponse.Context);
            try
            {
                await this.teamsFileUpload.UploadToOneDrive(
                    fileName,
                    fileConsentCardResponse.UploadInfo.UploadUrl,
                    cancellationToken);

                await this.teamsFileUpload.FileUploadCompletedAsync(
                    turnContext,
                    fileConsentCardResponse,
                    fileName,
                    notificationId,
                    cancellationToken);
            }
            catch (Exception e)
            {
                await this.teamsFileUpload.FileUploadFailedAsync(
                    turnContext,
                    notificationId,
                    e.ToString(),
                    cancellationToken);
            }
        }

        /// <summary>
        /// Invoke when a file upload decline consent activitiy is received from the channel.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="fileConsentCardResponse">The declined response object of File Card.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task reprsenting asynchronous operation.</returns>
        protected override async Task OnTeamsFileConsentDeclineAsync(ITurnContext<IInvokeActivity> turnContext, FileConsentCardResponse fileConsentCardResponse, CancellationToken cancellationToken)
        {
            var (fileName, notificationId) = this.teamsFileUpload.ExtractInformation(
                fileConsentCardResponse.Context);

            await this.teamsFileUpload.CleanUp(
                turnContext,
                fileName,
                notificationId,
                cancellationToken);

            var reply = MessageFactory.Text(this.localizer.GetString("PermissionDeclinedText"));
            reply.TextFormat = "xml";
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        /// <summary>
        /// Captures the user's reaction.
        /// </summary>
        /// <param name="messageReactions"> Reactions user's.</param>
        /// <param name="turnContext"> The context object for this turn.</param>
        /// <param name="cancellationToken"> A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnReactionsAddedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            var reactMessage = new ReactionMessageDataEntity();

            foreach (var reaction in messageReactions)
            {
                // Id Message
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.ReplyToId))
                {
                    reactMessage.MessageId = turnContext.Activity.ReplyToId;
                }

                // Reaction
                reactMessage.Reaction = reaction.Type;

                // Channel
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.ChannelId))
                {
                    reactMessage.ChannelId = turnContext.Activity.ChannelId;
                }

                // Id Conversation
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
                {
                    var var02 = turnContext.Activity.Conversation.Id;
                    var var03 = var02.Split(';');
                    if (var03.Length > 0)
                    {
                        reactMessage.ConversationId = var03[0];
                    }
                }

                // Tenant
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.TenantId))
                {
                    reactMessage.Tenant = turnContext.Activity.Conversation.TenantId;
                }

                // Is Group
                reactMessage.IsGroup = turnContext.Activity.Conversation.IsGroup.GetValueOrDefault();

                // Conversation Type
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.ConversationType))
                {
                    reactMessage.ConversationType = turnContext.Activity.Conversation.ConversationType;
                }

                reactMessage.AddReaction = true;
                reactMessage.IsProcessed = false;

                var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);

                // AadObjectId
                if (!string.IsNullOrWhiteSpace(member.AadObjectId))
                {
                    reactMessage.AadObjectId = member.AadObjectId;
                }

                // Email
                if (!string.IsNullOrWhiteSpace(member.Email))
                {
                    reactMessage.Email = member.Email;
                }

                // GivenName
                if (!string.IsNullOrWhiteSpace(member.GivenName))
                {
                    reactMessage.GivenName = member.GivenName;
                }

                // Name
                if (!string.IsNullOrWhiteSpace(member.Name))
                {
                    reactMessage.Name = member.Name;
                }

                // Role
                if (!string.IsNullOrWhiteSpace(member.Role))
                {
                    reactMessage.Role = member.Role;
                }

                // SurName
                if (!string.IsNullOrWhiteSpace(member.Surname))
                {
                    reactMessage.SurName = member.Surname;
                }

                // UserPrincipalName
                if (!string.IsNullOrWhiteSpace(member.UserPrincipalName))
                {
                    reactMessage.UserPrincipalName = member.UserPrincipalName;
                }

                // UserRole
                if (!string.IsNullOrWhiteSpace(member.UserRole))
                {
                    reactMessage.UserRole = member.UserRole;
                }

                var temp = await this.reactionMessageDataRepository.CreateReactionData(reactMessage);
            }
        }

        /// <summary>
        /// Capture the user's reaction removal.
        /// </summary>
        /// <param name="messageReactions"> Reactions user's.</param>
        /// <param name="turnContext"> The context object for this turn.</param>
        /// <param name="cancellationToken"> A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnReactionsRemovedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            var reactMessage = new ReactionMessageDataEntity();

            foreach (var reaction in messageReactions)
            {
                // Id Message
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.ReplyToId))
                {
                    reactMessage.MessageId = turnContext.Activity.ReplyToId;
                }

                // Reaction
                reactMessage.Reaction = reaction.Type;

                // Channel
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.ChannelId))
                {
                    reactMessage.ChannelId = turnContext.Activity.ChannelId;
                }

                // Id Conversation
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.Id))
                {
                    var var02 = turnContext.Activity.Conversation.Id;
                    var var03 = var02.Split(';');
                    if (var03.Length > 0)
                    {
                        reactMessage.ConversationId = var03[0];
                    }
                }

                // Tenant
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.TenantId))
                {
                    reactMessage.Tenant = turnContext.Activity.Conversation.TenantId;
                }

                // Is Group
                reactMessage.IsGroup = turnContext.Activity.Conversation.IsGroup.GetValueOrDefault();

                // Conversation Type
                if (!string.IsNullOrWhiteSpace(turnContext.Activity.Conversation.ConversationType))
                {
                    reactMessage.ConversationType = turnContext.Activity.Conversation.ConversationType;
                }

                reactMessage.AddReaction = false;
                reactMessage.IsProcessed = false;

                var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);

                // AadObjectId
                if (!string.IsNullOrWhiteSpace(member.AadObjectId))
                {
                    reactMessage.AadObjectId = member.AadObjectId;
                }

                // Email
                if (!string.IsNullOrWhiteSpace(member.Email))
                {
                    reactMessage.Email = member.Email;
                }

                // GivenName
                if (!string.IsNullOrWhiteSpace(member.GivenName))
                {
                    reactMessage.GivenName = member.GivenName;
                }

                // Name
                if (!string.IsNullOrWhiteSpace(member.Name))
                {
                    reactMessage.Name = member.Name;
                }

                // Role
                if (!string.IsNullOrWhiteSpace(member.Role))
                {
                    reactMessage.Role = member.Role;
                }

                // SurName
                if (!string.IsNullOrWhiteSpace(member.Surname))
                {
                    reactMessage.SurName = member.Surname;
                }

                // UserPrincipalName
                if (!string.IsNullOrWhiteSpace(member.UserPrincipalName))
                {
                    reactMessage.UserPrincipalName = member.UserPrincipalName;
                }

                // UserRole
                if (!string.IsNullOrWhiteSpace(member.UserRole))
                {
                    reactMessage.UserRole = member.UserRole;
                }

                var temp = await this.reactionMessageDataRepository.CreateReactionData(reactMessage);
            }
        }

        /// <summary>
        /// Send notification about Bot not being monitored.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the message to be displayed.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await this.EnviarMensagemAsync(turnContext, cancellationToken, "Desculpe, mas esse Bot não está sendo monitorado!");
        }

        /// <summary>
        /// Verify status of conversation.
        /// </summary>
        /// <param name="activity">Update conversation.</param>
        /// <returns>Retunr a boolean value.</returns>
        private bool IsTeamInformationUpdated(IConversationUpdateActivity activity)
        {
            if (activity == null)
            {
                return false;
            }

            var channelData = activity.GetChannelData<TeamsChannelData>();
            if (channelData == null)
            {
                return false;
            }

            return CompanyCommunicatorBot.TeamRenamedEventType.Equals(channelData.EventType, StringComparison.OrdinalIgnoreCase);
        }

        private async Task EnviarMensagemAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string mensagem)
        {
            // Constroi a Mensagem
            var message = MessageFactory.Text($"{mensagem.Trim()}");

            // Envia a Mensagen
            var res = await turnContext.SendActivityAsync(message);
        }
    }
}