﻿// <copyright file="TemplateNotificationsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.CompanyCommunicator.Authentication;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.NotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.TeamData;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Resources;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.CompanyCommunicator.Models;
    using Microsoft.Teams.Apps.CompanyCommunicator.Repositories.Extensions;

    /// <summary>
    /// Controller for the Template notification data.
    /// </summary>
    [Route("api/templateNotifications")]
    [Authorize(PolicyNames.MustBeValidUpnPolicy)]
    public class TemplateNotificationsController : ControllerBase
    {
        private readonly NotificationDataRepository notificationDataRepository;
        private readonly TeamDataRepository teamDataRepository;
        private readonly IGroupsService groupsService;
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateNotificationsController"/> class.
        /// </summary>
        /// <param name="notificationDataRepository">Notification data repository instance.</param>
        /// <param name="teamDataRepository">Team data repository instance.</param>
        /// <param name="localizer">Localization service.</param>
        /// <param name="groupsService">group service.</param>
        public TemplateNotificationsController(
            NotificationDataRepository notificationDataRepository,
            TeamDataRepository teamDataRepository,
            IStringLocalizer<Strings> localizer,
            IGroupsService groupsService)
        {
            this.notificationDataRepository = notificationDataRepository;
            this.teamDataRepository = teamDataRepository;
            this.localizer = localizer;
            this.groupsService = groupsService;
        }

        /// <summary>
        /// Create a new Template notification.
        /// </summary>
        /// <param name="notification">A new Template Notification to be created.</param>
        /// <returns>The created notification's id.</returns>
        [HttpPost]
        public async Task<ActionResult<string>> CreateTemplateNotificationAsync([FromBody] TemplateNotification notification)
        {
            if (!notification.Validate(this.localizer, out string errorMessage))
            {
                return this.BadRequest(errorMessage);
            }

            var containsHiddenMembership = await this.groupsService.ContainsHiddenMembershipAsync(notification.Groups);
            if (containsHiddenMembership)
            {
                return this.Forbid();
            }

            var notificationId = await this.notificationDataRepository.CreateTemplateNotificationAsync(
                notification,
                this.HttpContext.User?.Identity?.Name);
            return this.Ok(notificationId);
        }

        /// <summary>
        /// Delete an existing Template notification.
        /// </summary>
        /// <param name="id">The id of the draft notification to be deleted.</param>
        /// <returns>If the passed in Id is invalid, it returns 404 not found error. Otherwise, it returns 200 OK.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplateNotificationAsync(string id)
        {
            var notificationEntity = await this.notificationDataRepository.GetAsync(
                NotificationDataTableNames.TemplateNotoficationsPartition,
                id);
            if (notificationEntity == null)
            {
                return this.NotFound();
            }

            await this.notificationDataRepository.DeleteAsync(notificationEntity);
            return this.Ok();
        }

        /// <summary>
        /// Get draft notifications.
        /// </summary>
        /// <returns>A list of <see cref="TemplateNotificationSummary"/> instances.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TemplateNotificationSummary>>> GetAllTemplateNotificationsAsync()
        {
            var notificationEntities = await this.notificationDataRepository.GetAllTemplateNotificationsAsync();

            var result = new List<TemplateNotificationSummary>();
            foreach (var notificationEntity in notificationEntities)
            {
                var summary = new TemplateNotificationSummary
                {
                    Id = notificationEntity.Id,
                    Title = notificationEntity.Title,
                    NmMensagem = notificationEntity.NmMensagem,
                };

                result.Add(summary);
            }

            return result;
        }

        /// <summary>
        /// Get a Template notification by Id.
        /// </summary>
        /// <param name="id">Template notification Id.</param>
        /// <returns>It returns the draft notification with the passed in id.
        /// The returning value is wrapped in a ActionResult object.
        /// If the passed in id is invalid, it returns 404 not found error.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateNotification>> GetTemplateNotificationByIdAsync(string id)
        {
            var notificationEntity = await this.notificationDataRepository.GetAsync(
                NotificationDataTableNames.TemplateNotoficationsPartition,
                id);
            if (notificationEntity == null)
            {
                return this.NotFound();
            }

            var result = new TemplateNotification
            {
                Id = notificationEntity.Id,
                Title = notificationEntity.Title,
                ImageLink = notificationEntity.ImageLink,
                Summary = notificationEntity.Summary,
                Author = notificationEntity.Author,
                ButtonTitle = notificationEntity.ButtonTitle,
                ButtonLink = notificationEntity.ButtonLink,
                CreatedDateTime = notificationEntity.CreatedDate,
                Teams = notificationEntity.Teams,
                Rosters = notificationEntity.Rosters,
                Groups = notificationEntity.Groups,
                AllUsers = notificationEntity.AllUsers,
                Template = notificationEntity.Template,
                Schedule = notificationEntity.Schedule,
                NmMensagem = notificationEntity.NmMensagem,
                HeaderImgLink = notificationEntity.HeaderImgLink,
                FooterImgLink = notificationEntity.FooterImgLink,
                ButtonLink2 = notificationEntity.ButtonLink2,
                ButtonTitle2 = notificationEntity.ButtonTitle2,
            };

            return this.Ok(result);
        }

        /// <summary>
        /// Update an existing Template notification.
        /// </summary>
        /// <param name="notification">An existing Draft Notification to be updated.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        [HttpPut]
        public async Task<IActionResult> UpdateTemplateNotificationAsync([FromBody] TemplateNotification notification)
        {
            var containsHiddenMembership = await this.groupsService.ContainsHiddenMembershipAsync(notification.Groups);
            if (containsHiddenMembership)
            {
                return this.Forbid();
            }

            if (!notification.Validate(this.localizer, out string errorMessage))
            {
                return this.BadRequest(errorMessage);
            }

            var notificationEntity = new NotificationDataEntity
            {
                PartitionKey = NotificationDataTableNames.TemplateNotoficationsPartition,
                RowKey = notification.Id,
                Id = notification.Id,
                Title = notification.Title,
                ImageLink = notification.ImageLink,
                Summary = notification.Summary,
                Author = notification.Author,
                ButtonTitle = notification.ButtonTitle,
                ButtonLink = notification.ButtonLink,
                CreatedBy = this.HttpContext.User?.Identity?.Name,
                CreatedDate = DateTime.UtcNow,
                IsDraft = true,
                Teams = notification.Teams,
                Rosters = notification.Rosters,
                Groups = notification.Groups,
                AllUsers = notification.AllUsers,
                Template = notification.Template,
                Schedule = notification.Schedule,
                NmMensagem = notification.NmMensagem,
                HeaderImgLink = notification.HeaderImgLink,
                FooterImgLink = notification.FooterImgLink,
                ButtonTitle2 = notification.ButtonTitle2,
                ButtonLink2 = notification.ButtonLink2,
            };

            await this.notificationDataRepository.CreateOrUpdateAsync(notificationEntity);
            return this.Ok();
        }

        /// <summary>
        /// Duplicate an existing draft notification.
        /// </summary>
        /// <param name="id">The id of a Template Notification to be duplicated.</param>
        /// <returns>If the passed in id is invalid, it returns 404 not found error. Otherwise, it returns 200 OK.</returns>
        [HttpPost("duplicates/{id}")]
        public async Task<IActionResult> DuplicateTemplateNotificationAsync(string id)
        {
            var notificationEntity = await this.FindNotificationToDuplicate(id);
            if (notificationEntity == null)
            {
                return this.NotFound();
            }

            var createdBy = this.HttpContext.User?.Identity?.Name;
            notificationEntity.Title = this.localizer.GetString("DuplicateText", notificationEntity.Title);
            await this.notificationDataRepository.DuplicateTemplateNotificationAsync(notificationEntity, createdBy);

            return this.Ok();
        }

        private async Task<NotificationDataEntity> FindNotificationToDuplicate(string notificationId)
        {
            var notificationEntity = await this.notificationDataRepository.GetAsync(
                NotificationDataTableNames.TemplateNotoficationsPartition,
                notificationId);
            if (notificationEntity == null)
            {
                notificationEntity = await this.notificationDataRepository.GetAsync(
                    NotificationDataTableNames.DraftNotificationsPartition,
                    notificationId);
            }

            return notificationEntity;
        }
    }
}
