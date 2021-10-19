// <copyright file="ImgRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.CompanyCommunicator.ImageConv
{
    /// <summary>
    /// Options used for creating repositories.
    /// </summary>
    public class ImgRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImgRepository"/> class.
        /// </summary>
        public ImgRepository()
        {
            // Default this option to true as ensuring the table exists is technically
            // more robust.
            this.EnsureTableExists = true;
        }

        /// <summary>
        /// Gets or sets the storage account connection string.
        /// </summary>
        public string StorageAccountConnectionString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the table should be created
        /// if it does not already exist.
        /// </summary>
        public bool EnsureTableExists { get; set; }
    }
}
