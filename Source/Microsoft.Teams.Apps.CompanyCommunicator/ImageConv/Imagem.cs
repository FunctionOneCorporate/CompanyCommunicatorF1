// <copyright file="Imagem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.CompanyCommunicator.ImageConv
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::Azure.Storage.Blobs;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.CompanyCommunicator.Models;

    /// <summary>
    /// Imagem service.
    /// </summary>
    public class Imagem : IImagem
    {
        private readonly string strConn;

        /// <summary>
        /// Initializes a new instance of the <see cref="Imagem"/> class.
        /// </summary>
        /// <param name="_repositoryOptions">recebe valor do campo storage.</param>
        public Imagem(IOptions<ImgRepository> _repositoryOptions)
        {
            this.strConn = _repositoryOptions.Value.StorageAccountConnectionString.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadBase64Image"/> class.
        /// </summary>
        /// <param name="img">recebe valor do campo storage.</param>
        /// <returns>Return string image.</returns>
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task<string> UploadBase64Image(ImgData img)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            string container = Common.Constants.BlobContainerImageName;
            string base64Image = img.Img;
            string retorno = string.Empty;

            // Gera um nome randomico para imagem
            var fileName = Guid.NewGuid().ToString() + ".jpg";

            // Limpa o hash enviado
            var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(base64Image, string.Empty);

            // Gera um array de Bytes
            byte[] imageBytes = Convert.FromBase64String(data);

            // define o blob
            var blobClient = new BlobClient(this.strConn, container, fileName);

            // Envia a imagem
            using (var stream = new System.IO.MemoryStream(imageBytes))
            {
                blobClient.Upload(stream);
            }

            // Retorna a URL da imagem
            return blobClient.Uri.AbsoluteUri.ToString();

            // return "ok";
        }
    }
}
