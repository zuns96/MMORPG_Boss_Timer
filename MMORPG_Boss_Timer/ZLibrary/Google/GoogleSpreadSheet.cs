﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ZLibrary.Google
{
    public static class GoogleSpreadSheet
    {
        private static string[] s_scopes = { SheetsService.Scope.SpreadsheetsReadonly };

        public static Dictionary<int, T> ConvertTableData<T>(string spreadSheetId, string range) where T : MasterData
        {
            Dictionary<int, T> dic = new Dictionary<int, T>();
            UserCredential credential;

            using (var stream = new FileStream(GoogleDefine.c_credentials_path, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                const string credPath = "token.json";
                const string user = "user";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    s_scopes,
                    user,
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Sheets API service.
            var initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "appName",
            };

            using (var service = new SheetsService(initializer))
            {
                SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadSheetId, range);
                ValueRange response = request.Execute();

                IList<IList<Object>> values = response.Values;
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        T data = Activator.CreateInstance(typeof(T), row) as T;
                        dic.Add(data.PID, data);
                    }
                }
            }
            
            return dic;
        }
    }
}
