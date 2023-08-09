namespace Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Extensions;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.UserData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Resources;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.CompanyCommunicator.Prep.Func.PreparingToSend.Extensions;
    using Newtonsoft.Json;



    //private readonly UserDataRepository userDataRepository;
    //private readonly SentNotificationDataRepository sentNotificationDataRepository;
    //private readonly IUsersService usersService;
    public class UserDataTableSync
    {

        private readonly UserDataRepository userDataRepository;
        private readonly IGroupMembersService groupMembersService;
        private readonly IUsersService usersService;
        private readonly IAppManagerService appManagerService;
        private readonly IChatsService chatsService;
        private readonly IAppSettingsService appSettingsService;
        private readonly IStringLocalizer<Strings> localizer;
        private  string appId;
        private  string tenantId;
        private  string serviceUrl;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncAllUsersActivity"/> class.
        /// </summary>
        /// <param name="userDataRepository">User Data repository.</param>
        /// <param name="usersService">Users service.</param>

        public UserDataTableSync(
            UserDataRepository userDataRepository, IUsersService usersService, IAppManagerService appManagerService,
            IChatsService chatsService, IAppSettingsService appSettingsService,IGroupMembersService GroupMembersService, IStringLocalizer<Strings> localize)
        {
            if (appManagerService is null)
            {
                throw new ArgumentNullException(nameof(appManagerService));
            }

            if (chatsService is null)
            {
                throw new ArgumentNullException(nameof(chatsService));
            }
            this.groupMembersService = GroupMembersService ?? throw new ArgumentNullException(nameof(GroupMembersService));
            this.chatsService = chatsService ?? throw new ArgumentNullException(nameof(chatsService));
            this.userDataRepository = userDataRepository ?? throw new ArgumentNullException(nameof(userDataRepository));
            this.usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
            this.localizer = localize ?? throw new ArgumentNullException(nameof(localize));
            this.appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
            this.appManagerService = appManagerService ?? throw new ArgumentNullException(nameof(appManagerService));
        }

        [FunctionName("UserDataTableSync")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando sincronizacao de usuarios.");
        
            this.appId =  await this.appSettingsService.GetUserAppIdAsync();
            this.serviceUrl = await this.appSettingsService.GetServiceUrlAsync();
            this.tenantId = Environment.GetEnvironmentVariable("TenantId");
            log.LogInformation($"appId: {this.appId}, tennantId: {this.tenantId}, serviceUrl: {this.serviceUrl}");
            //string name = req.Query["name"];
           
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation($"data: {data}");
            string groupID = data?.groupID;
            if (!groupID.IsNullOrEmpty())
            {
                log.LogInformation($"Iniciando sincronizacao de usuarios do grupo {groupID}.");
                var users = await this.groupMembersService.GetGroupMembersAsync(groupID);
                var maxParallelism = Math.Min(users.Count(), 30);
                await users.ForEachAsync(maxParallelism, body: async entry => {
                    await this.ProcessUserAsync(entry, log);
                });
                return new OkObjectResult($"sincronizados: {users.Count()}");
            }
            await this.SyncAllUsers("1", log);
            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";



            return new OkObjectResult("ok");
        }

        /// <summary>
        /// Syncs delta changes only.
        /// </summary>
        private async Task SyncAllUsers(string userId, ILogger log)
        {
            // Sync users
            var deltaLink = await this.userDataRepository.GetDeltaLinkAsync();

            (IEnumerable<User>, string) tuple = (new List<User>(), string.Empty);
            try
            {
                tuple = await this.usersService.GetAllUsersAsync(deltaLink);
            }
            catch (ServiceException exception)
            {
                var errorMessage = this.localizer.GetString("FailedToGetAllUsersFormat", exception.StatusCode, exception.Message);
                UserDataEntity userDataEntity = new UserDataEntity()
                {
                    PartitionKey = UserDataTableNames.UserDataPartition,
                    RowKey = userId,
                    ConversationId = errorMessage,
                };
                await this.userDataRepository.InsertOrMergeAsync(userDataEntity);
                return;
            }

            // process users.
            var users = tuple.Item1;
            if (!users.IsNullOrEmpty())
            {
                log.LogInformation($"Existem {users.Count()} para sincronização, aguarde...");
                var maxParallelism = Math.Min(users.Count(), 30);
                await users.ForEachAsync(maxParallelism, body: async entry => {
                    await this.ProcessUserAsync(entry, log);
                });
            }

            // Store delta link
            if (!string.IsNullOrEmpty(tuple.Item2))
            {
                await this.userDataRepository.SetDeltaLinkAsync(tuple.Item2);
            }
        }

        private async Task ProcessUserAsync(User user, ILogger log)
        {
            // Delete users who were removed.
            log.LogInformation($"Processando usuario {user.DisplayName}.");
            if( user.DisplayName.IsNullOrEmpty())
            {
                log.LogInformation($"Usuario {user.Id} não possui nome.");
                return;
            }
            if (user.AdditionalData?.ContainsKey("@removed") == true)
            {
                log.LogInformation($"Removendo usuario {user.DisplayName}.");
                var localUser = await this.userDataRepository.GetAsync(UserDataTableNames.UserDataPartition, user.Id);
                if (localUser != null)
                {
                    await this.userDataRepository.DeleteAsync(localUser);
                }

                return;
            }

            // skip Guest users.
            if (string.Equals(user.UserType, "Guest", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            string conversationId = "";
            // skip users who do not have teams license.
            try
            {
                var hasTeamsLicense = this.usersService.ValidTeamsLicense(user);
                if (!hasTeamsLicense)
                {
                    log.LogInformation($"Usuario {user.DisplayName} não possui licença do Teams.");
                   
                    return;
                }
                log.LogInformation($"Instalando app para usuario {user.DisplayName}.");
                conversationId = await this.InstallAppAndGetConversationId(user.Id, log);
            }
            catch (ServiceException ex)
            {
                // Failed to get user's license details. Will skip the user.
                log.LogInformation($"Falha ao obter licença do usuario {user.DisplayName}. - erro: {ex.Message}");
                return;
            }

            // Store user.
            await this.userDataRepository.InsertOrMergeAsync(
                new UserDataEntity()
                {
                    PartitionKey = UserDataTableNames.UserDataPartition,
                    RowKey = user.Id,
                    AadId = user.Id,
                    ConversationId = conversationId,
                    TenantId = this.tenantId,
                    ServiceUrl = this.serviceUrl,
                });
        }
        private async Task<string> InstallAppAndGetConversationId(string RecipientId, ILogger log)
        {
           
            if (string.IsNullOrEmpty(this.appId))
            {
                log.LogError("User app id not available.");
                return string.Empty;
            }

            // Install app.
            try
            {
                bool isAppInstalled = await this.appManagerService.IsAppInstalledForUserAsync(this.appId, RecipientId);
                if(isAppInstalled)
                {
                    log.LogInformation($"App já instalado para usuario {RecipientId}.");
                   // var appTokenId = await this.appManagerService.GetAppInstallationIdForUserAsync(this.appId, RecipientId);
                    //log.LogInformation($"InstalationID: {appTokenId}. - Usuario: {RecipientId}");
                    return await this.chatsService.GetChatThreadIdAsync(RecipientId, this.appId);
                }
                await this.appManagerService.InstallAppForUserAsync(this.appId, RecipientId);
                log.LogInformation($"App instalado com sucesso para usuario {RecipientId}.");
            }
            catch (ServiceException exception)
            {
                switch (exception.StatusCode)
                {
                    case HttpStatusCode.Conflict:
                        // Note: application is already installed, we should fetch conversation id for this user.
                        log.LogWarning("Application is already installed for the user.");
                        break;

                    default:
                        var errorMessage = this.localizer.GetString("FailedToInstallApplicationForUserFormat", RecipientId, exception.Message);
                       log.LogError(exception, errorMessage);
                        //await this.notificationDataRepository.SaveWarningInNotificationDataEntityAsync(notificationId, errorMessage);
                        return string.Empty;
                }
            }

            // Get conversation id.
            try
            {
                //var appTokenId = await this.appManagerService.GetAppInstallationIdForUserAsync(this.appId, RecipientId);
                return await this.chatsService.GetChatThreadIdAsync(RecipientId, this.appId);
            }
            catch (ServiceException exception)
            {
                var errorMessage = this.localizer.GetString("FailedToGetConversationForUserFormat", RecipientId, exception.StatusCode, exception.Message);
                log.LogError(exception, errorMessage);
                //await this.notificationDataRepository.SaveWarningInNotificationDataEntityAsync(notificationId, errorMessage);
                return string.Empty;
            }
        }
    }
}
