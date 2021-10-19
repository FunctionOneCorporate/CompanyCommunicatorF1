// <copyright file="ImageController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.CompanyCommunicator.Authentication;
    using Microsoft.Teams.Apps.CompanyCommunicator.Common.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.CompanyCommunicator.ImageConv;
    using Microsoft.Teams.Apps.CompanyCommunicator.Models;

    /// <summary>
    /// Controller for the Image.
    /// </summary>
    [Route("api/image")]
     public class ImageController : Controller
    {
        private readonly IImagem imagemConv;
        private readonly SentNotificationDataRepository sentNotificationDataRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageController"/> class.
        /// </summary>
        /// <param name="imagem">Image from client.</param>
        /// <param name="sentNotification"> Functions of manipulation data sent.</param>
        public ImageController(IImagem imagem, SentNotificationDataRepository sentNotification)
        {
            this.imagemConv = imagem;
            this.sentNotificationDataRepository = sentNotification;
        }

        /// <summary>
        /// Get a Template notification by Id.
        /// </summary>
        /// <param name="imgData">Template notification Id.</param>
        /// <returns>It returns the draft notification with the passed in id.
        /// The returning value is wrapped in a ActionResult object.
        /// If the passed in id is invalid, it returns 404 not found error.</returns>
        [HttpPost]
        [Authorize(PolicyNames.MustBeValidUpnPolicy)]
        public async Task<ActionResult<string>> UpdImg([FromBody] ImgData imgData)
        {
            try
            {
                var imagem = await this.imagemConv.UploadBase64Image(imgData);
                return this.Ok(imagem);
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get image and register solicitation user.
        /// </summary>
        /// <param name="aadid"> Id user or id Group Teams.</param>
        /// <param name="notificationId"> Id message in Company Communicator.</param>
        /// <returns> Returns url image. </returns>
        [HttpGet]
        public async Task<ActionResult> GetImageValidation([FromQuery] string aadid, [FromQuery] string notificationId)
        {
            string img = "iVBORw0KGgoAAAANSUhEUgAAA+gAAAABCAIAAADCYhNkAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAAAbSURBVEhL7cExAQAADMOg+Ted2egBXAAAwLh6Wj6s7rTFGj8AAAAASUVORK5CYII=";
            byte[] data = Convert.FromBase64String(img);
            this.Response.ContentType = "image/png";
            this.Response.ContentLength = data.Length;
            this.Response.StatusCode = 200;
            Stream stream = new MemoryStream(data);


            if (string.IsNullOrEmpty(aadid) || string.IsNullOrEmpty(notificationId))
            {
                return new FileStreamResult(stream, new Net.Http.Headers.MediaTypeHeaderValue("image/png"));
            }

            try
            {
                var sentnotification = await this.sentNotificationDataRepository.GetAsync(notificationId, aadid);

                if (sentnotification != null)
                {
                    if (sentnotification.RecipientType == "User")
                    {
                        sentnotification.QtdAcesso = 1;
                        sentnotification.DateVis = DateTime.UtcNow;
                    }
                    else
                    {
                        sentnotification.QtdAcesso++;
                        sentnotification.DateVis = DateTime.UtcNow;
                    }

                    await this.sentNotificationDataRepository.InsertOrMergeAsync(sentnotification);
                    return new FileStreamResult(stream, new Net.Http.Headers.MediaTypeHeaderValue("image/png"));
                }
                else
                {
                    return new FileStreamResult(stream, new Net.Http.Headers.MediaTypeHeaderValue("image/png"));
                }

            } catch (Exception e)
            {
                //return null;
                return new FileStreamResult(stream, new Net.Http.Headers.MediaTypeHeaderValue("image/png"));
            }
        }
    }
}
