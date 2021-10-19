// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
[assembly: Microsoft.Azure.Functions.Extensions.DependencyInjection.FunctionsStartup(
    typeof(Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Startup))]

namespace Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func
{
    extern alias BetaLib;

    using System;
    using System.Globalization;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.ExportData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.ReactionMessageData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamsRatingAnalytics;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.UserData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.AdaptiveCard;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.CommonBot;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.DataQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.ExportQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.PrepareToSendQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MessageQueues.SendQueue;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.Teams;
    using Microsoft.Teams.Apps.CompanyCommunicator.Schedule.Func.Services;

    using Beta = BetaLib:: Microsoft.Graph;

    /// <summary>
    /// Register services in DI container of the Azure functions system.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Add all options set from configuration values.
            builder.Services.AddOptions<ScheduleFunctionOptions>()
                .Configure<IConfiguration>((scheduleFunctionOptions, configuration) =>
                {
                    scheduleFunctionOptions.MaxNumberOfAttempts =
                        configuration.GetValue<int>("MaxNumberOfAttempts", 1);

                    scheduleFunctionOptions.SendRetryDelayNumberOfSeconds =
                        configuration.GetValue<double>("SendRetryDelayNumberOfSeconds", 660);
                });

            // Add all options set from configuration values.
            builder.Services.AddOptions<RepositoryOptions>()
                .Configure<IConfiguration>((repositoryOptions, configuration) =>
                {
                    repositoryOptions.StorageAccountConnectionString =
                        configuration.GetValue<string>("AzureWebJobsStorage");

                    // Defaulting this value to true because the main app should ensure all
                    // tables exist. It is here as a possible configuration setting in
                    // case it needs to be set differently.
                    repositoryOptions.EnsureTableExists =
                        !configuration.GetValue<bool>("IsItExpectedThatTableAlreadyExists", true);
                });
            builder.Services.AddOptions<MessageQueueOptions>()
                .Configure<IConfiguration>((messageQueueOptions, configuration) =>
                {
                    messageQueueOptions.ServiceBusConnection =
                        configuration.GetValue<string>("ServiceBusConnection");
                });
            builder.Services.AddOptions<BotOptions>()
                .Configure<IConfiguration>((botOptions, configuration) =>
                {
                    botOptions.MicrosoftAppId =
                        configuration.GetValue<string>("MicrosoftAppId");
                    botOptions.MicrosoftAppPassword =
                        configuration.GetValue<string>("MicrosoftAppPassword");
                });
            builder.Services.AddOptions<RepositoryOptions>()
                .Configure<IConfiguration>((repositoryOptions, configuration) =>
                {
                    repositoryOptions.StorageAccountConnectionString =
                        configuration.GetValue<string>("AzureWebJobsStorage");

                    // Defaulting this value to true because the main app should ensure all
                    // tables exist. It is here as a possible configuration setting in
                    // case it needs to be set differently.
                    repositoryOptions.EnsureTableExists =
                        !configuration.GetValue<bool>("IsItExpectedThatTableAlreadyExists", true);
                });

            builder.Services.AddOptions<ConfidentialClientApplicationOptions>().
                Configure<IConfiguration>((confidentialClientApplicationOptions, configuration) =>
                {
                    confidentialClientApplicationOptions.ClientId = configuration.GetValue<string>("MicrosoftAppId");
                    confidentialClientApplicationOptions.ClientSecret = configuration.GetValue<string>("MicrosoftAppPassword");
                    confidentialClientApplicationOptions.TenantId = configuration.GetValue<string>("TenantId");
                });

            builder.Services.AddLocalization();

            // Set current culture.
            var culture = Environment.GetEnvironmentVariable("i18n:DefaultCulture");

            // Add repositories.
            builder.Services.AddSingleton<NotificationDataRepository>();
            builder.Services.AddSingleton<SendingNotificationDataRepository>();
            builder.Services.AddSingleton<SentNotificationDataRepository>();
            builder.Services.AddSingleton<UserDataRepository>();
            builder.Services.AddSingleton<TeamDataRepository>();
            builder.Services.AddSingleton<ExportDataRepository>();
            builder.Services.AddSingleton<AppConfigRepository>();
            builder.Services.AddSingleton<GlobalSendingNotificationDataRepository>();
            builder.Services.AddSingleton<TeamsRatingAnalyticsDataRepository>();
            builder.Services.AddSingleton<ReactionMessageDataRepository>();

            // Add service bus message queues.
            builder.Services.AddSingleton<SendQueue>();
            builder.Services.AddSingleton<DataQueue>();
            builder.Services.AddSingleton<ExportQueue>();
            builder.Services.AddSingleton<PrepareToSendQueue>();

            // Add miscellaneous dependencies.
            builder.Services.AddTransient<TableRowKeyGenerator>();
            builder.Services.AddTransient<AdaptiveCardCreator>();
            builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();

            // Add Teams services.
            builder.Services.AddTransient<ITeamMembersService, TeamMembersService>();
            builder.Services.AddTransient<IConversationService, ConversationService>();
            builder.Services.AddTransient<IMessageService, MessageService>();

            // Add graph services.
            this.AddGraphServices(builder);

            builder.Services.AddTransient<INotificationService, NotificationService>();
            builder.Services.AddTransient<ITeamsRatingService, TeamsRatingService>();
            builder.Services.AddTransient<IReactionMessageService, ReactionMessageService>();
        }

        /// <summary>
        /// Adds Graph Services and related dependencies.
        /// </summary>
        /// <param name="builder">Builder.</param>
        private void AddGraphServices(IFunctionsHostBuilder builder)
        {
            // Options
            builder.Services.AddOptions<ConfidentialClientApplicationOptions>().
                Configure<IConfiguration>((confidentialClientApplicationOptions, configuration) =>
                {
                    confidentialClientApplicationOptions.ClientId = configuration.GetValue<string>("MicrosoftAppId");
                    confidentialClientApplicationOptions.ClientSecret = configuration.GetValue<string>("MicrosoftAppPassword");
                    confidentialClientApplicationOptions.TenantId = configuration.GetValue<string>("TenantId");
                });

            // Graph Token Services
            builder.Services.AddSingleton<IConfidentialClientApplication>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<ConfidentialClientApplicationOptions>>();
                return ConfidentialClientApplicationBuilder
                    .Create(options.Value.ClientId)
                    .WithClientSecret(options.Value.ClientSecret)
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{options.Value.TenantId}"))
                    .Build();
            });

            builder.Services.AddSingleton<IAuthenticationProvider, MsalAuthenticationProvider>();

            // Add Service Factory
            builder.Services.AddSingleton<IGraphServiceFactory, GraphServiceFactory>();

            // Add Graph Services
            builder.Services.AddScoped<IUsersService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetUsersService());
            builder.Services.AddScoped<IGroupMembersService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetGroupMembersService());
            builder.Services.AddScoped<IAppManagerService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetAppManagerService());
            builder.Services.AddScoped<IChatsService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetChatsService());
        }
    }
}
