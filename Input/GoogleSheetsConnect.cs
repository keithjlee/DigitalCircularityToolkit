using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System;
using Google.Apis.Auth.OAuth2.Flows;

namespace DigitalCircularityToolkit.Input
{
    public class GoogleSheetsConnect
    {
        private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets }; // Change scope for read/write
        private readonly string ApplicationName = "Your Application Name";
        private UserCredential credential;

        public GoogleSheetsConnect(string clientSecretFilePath)
        {
            Authenticate(clientSecretFilePath);
        }

        private void Authenticate(string clientSecretFilePath)
        {
            using (var stream = new FileStream(clientSecretFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user", CancellationToken.None).Result; // Synchronous call for simplicity
            }
        }


        /*private void Authenticate(string clientSecretFilePath)
        {
            using (var stream = new FileStream(clientSecretFilePath, FileMode.Open, FileAccess.Read))
            {
                // Initialize the authorization request
                var initializer = new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes = Scopes
                };

                var flow = new GoogleAuthorizationCodeFlow(initializer);

                // Create a code receiver that forces a new prompt
                var codeReceiver = new LocalServerCodeReceiver();

                // Generate new token
                credential = new AuthorizationCodeInstalledApp(flow, codeReceiver).AuthorizeAsync("user", CancellationToken.None).Result;
            }
        }*/


        public IList<IList<Object>> ReadSheetData(string spreadsheetId, string range)
        {
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response = request.Execute();
            return response.Values;
        }

        // Method to write data to the sheet
        public void WriteSheetData(string spreadsheetId, string range, IList<IList<Object>> values)
        {
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var valueRange = new ValueRange();
            valueRange.Values = values;

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            updateRequest.Execute();
        }
    }
}
