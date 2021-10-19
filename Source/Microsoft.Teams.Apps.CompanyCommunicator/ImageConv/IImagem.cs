// <copyright file="IImagem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.ImageConv
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.CompanyCommunicator.Models;

    /// <summary>
    /// Interface IImagem insert imagem in Cloud Blob.
    /// </summary>
    public interface IImagem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadBase64Image"/> class.
        /// </summary>
        /// <param name="img">The bot options.</param>
        /// <returns>Return string image.</returns>
        public Task<string> UploadBase64Image(ImgData img);
    }
}
