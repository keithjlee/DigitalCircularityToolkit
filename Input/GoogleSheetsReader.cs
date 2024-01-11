using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Google
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using System.IO;
using System.Threading;

namespace DigitalCircularityToolkit.Input
{
    public class GoogleSheetsReader
    {
        private readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private readonly string ApplicationName = "Your Application Name";
        private UserCredential credential;

        public GoogleSheetsReader(string clientSecretFilePath)
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
    }
}
