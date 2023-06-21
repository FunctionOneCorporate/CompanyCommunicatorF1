// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Microsoft.Extensions.Configuration;
using System;
using global::Azure.Identity;

namespace Microsoft.Teams.Apps.CompanyCommunicator
{
    /// <summary>
    /// Program class of the company communicator application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main function of the company communicator application.
        /// It builds a web host, then launches the company communicator into it.
        /// </summary>
        /// <param name="args">Arguments passed in to the function.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Create the web host builder.
        /// </summary>
        /// <param name="args">Arguments passed into the main function.</param>
        /// <returns>A web host builder instance.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
.ConfigureAppConfiguration((context, config) =>
{
var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
config.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
})
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
               });
    }
}