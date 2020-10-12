using System;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;

namespace LO3
{
    class Program
    {
        private static DriveService _service;
        private static string _token;

        static void Main(string[] args)
        {
            init();
        }

        static void init() {
            string[] scopes = new string[]{
                DriveService.Scope.Drive,
                DriveService.Scope.DriveFile
            };

            var clientId = "496441267406-3jim969787m2vfo76covlmph9dqh2icp.apps.googleusercontent.com";
            var clientSecret = "eJ0b84etLq-zD3nNS8Z9IAWl";

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets 
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                scopes,
                Environment.UserName,
                CancellationToken.None,
                    
                null
                ).Result; 

            _service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            _token = credential.Token.AccessToken;
                
            Console.WriteLine("Token: " + _token);

        }
    }
}
