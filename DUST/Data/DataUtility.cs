using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Data
{
    public static class DataUtility
    {
        private static int company1Id;
        private static int company2Id;
        private static int company3Id;
        private static int company4Id;
        private static int company5Id;

        /// <summary>
        /// Gets the connection string either from a remote database url or from appsettings.json
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static string GetConnectionString(IConfiguration configuration)
        {
            var appSettingsConnectionString = configuration.GetConnectionString("DefaultConnection");
            var remoteDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            return string.IsNullOrEmpty(remoteDbUrl) ? appSettingsConnectionString : BuildConnectionString(remoteDbUrl);
        }

        public static string BuildConnectionString(string databaseUrl)
        {
            // Get an object representation of the URI
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };

            return connectionStringBuilder.ToString();
        }

        public static async Task ManageDataAsync(IHost host)
        {
            throw new NotImplementedException();
        }

    }
}
