// <copyright file="GroupMembersService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MicrosoftGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Polly;

    /// <summary>
    /// Group Members Service.
    /// This gets the groups transitive members.
    /// </summary>
    internal class GroupMembersService : IGroupMembersService
    {
        private readonly IGraphServiceClient graphServiceClient;
        //private readonly IUsersService usersService;
        private const string TeamsLicenseId = "57ff2da0-773e-42df-b2af-ffb7a2317929";
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMembersService"/> class.
        /// </summary>
        /// <param name="graphServiceClient">graph service client.</param>
        internal GroupMembersService(IGraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient ?? throw new ArgumentNullException(nameof(graphServiceClient));
           // this.usersService = UsersService ?? throw new ArgumentNullException(nameof(UsersService));
        }

        /// <summary>
        /// get group members page by id.
        /// </summary>
        /// <param name="groupId">group id.</param>
        /// <returns>group members page.</returns>
        public async Task<IGroupTransitiveMembersCollectionWithReferencesPage> GetGroupMembersPageByIdAsync(string groupId)
        {
            var ret = await this.graphServiceClient
                                    .Groups[groupId]
                                    .TransitiveMembers
                                    .Request().Header("ConsistencyLevel", "eventual")
                                    .Top(GraphConstants.MaxPageSize)
                                    .WithMaxRetry(GraphConstants.MaxRetry)
                                    .GetAsync();
            return ret;
        }

        /// <summary>
        /// get group members page by next page ur;.
        /// </summary>
        /// <param name="groupMembersRef">group members page reference.</param>
        /// <param name="nextPageUrl">group members next page data link url.</param>
        /// <returns>group members page.</returns>
        public async Task<IGroupTransitiveMembersCollectionWithReferencesPage> GetGroupMembersNextPageAsnyc(
            IGroupTransitiveMembersCollectionWithReferencesPage groupMembersRef,
            string nextPageUrl)
        {
            groupMembersRef.InitializeNextPageRequest(this.graphServiceClient, nextPageUrl);
            return await groupMembersRef
                .NextPageRequest
                .GetAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetGroupMembersAsync(string groupId)
        {
            var response = await this.graphServiceClient
                                    .Groups[groupId]
                                    .TransitiveMembers
                                    .Request().Select("*")
                                    .Top(GraphConstants.MaxPageSize)
                                    .WithMaxRetry(GraphConstants.MaxRetry)
                                    .GetAsync();

            var users = response.OfType<User>().Where(x => x?.UserType == "Member").ToList();
          
            while (response.NextPageRequest != null)
            {
                response = await response.NextPageRequest.GetAsync();
                users?.AddRange(response.OfType<User>().Where(x => x?.UserType == "Member" ));
            }

            Console.WriteLine("Total de usuarios no grupo: " + users.Count);

            var licendUsers = users.Where(x => this.ValidTeamsLicense(x)).ToList();

            Console.WriteLine("Total de usuarios licenciados: " + licendUsers.Count);

            return licendUsers;
        }
       private bool ValidTeamsLicense(User user)
        {
            if (user.AssignedPlans != null)
            {
                return user.AssignedPlans.Any(x => x.ServicePlanId.ToString() == TeamsLicenseId && x.CapabilityStatus == "Enabled");
            }
            else
            {
                return false;
            }
        }
    }
}
