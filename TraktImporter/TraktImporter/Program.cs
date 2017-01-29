using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TraktApiSharp;
using TraktApiSharp.Enums;
using TraktApiSharp.Objects.Post.Syncs.Collection;
using TraktApiSharp.Objects.Post.Syncs.History;

namespace TraktImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () => await MainAsync()).GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            Console.WriteLine("Welcome to MyEpisodes2Trakt");
            Console.WriteLine();

            //TODO create an API app on trakt and add the keys here (use zapp thing as response, check both boxes)
            var clientId = "";
            var clientSecret = "";

            // grab titles from myepisodes page
            var path = @"D:\myep.txt"; //TODO copy *all* the data from the table here to a text file http://www.myepisodes.com/timewasted/ (skip header, summary)
            var titles = File.ReadAllLines(path).Select(x => x.Split('\t')[1].Trim()).ToList();
            Console.WriteLine($"MyEpisodes export loaded successfully, found {titles.Count()} titles.");
            Console.WriteLine();

            // try to get imdb IDs for these
            var omdbSearchUrl = "http://www.omdbapi.com/?type=series&s={0}";
            Func<string, string> getUrlForCurrent = x => string.Format(omdbSearchUrl, WebUtility.UrlEncode(x));
            var imdbIds = new Dictionary<string, string>();
            var saveFaileds = new List<string>();
            var hc = new HttpClient();
            foreach (var title in titles)
            {
                Console.Write(title.PadRight(50));
                var result = await hc.GetStringAsync(getUrlForCurrent(title));
                var jo = JsonConvert.DeserializeObject<OmdbResultRootObject>(result);
                if (jo.Response != "True" || jo.totalResults == "0")
                {
                    Console.WriteLine("Failed!");
                    saveFaileds.Add(title);
                }
                else
                {
                    Console.WriteLine($"Success, matched `{jo.Search[0].Title}`.");
                    imdbIds.Add(jo.Search[0].imdbID, title);
                }
            }

            // log in to trakt
            Console.WriteLine();
            var client = new TraktClient(clientId, clientSecret);
            var authUrl = client.OAuth.CreateAuthorizationUrl();
            Process.Start(authUrl);
            Console.Write("Please enter the code from the trakt website: ");
            var code = Console.ReadLine();
            client.Authentication.OAuthAuthorizationCode = code;
            var authorization = await client.OAuth.GetAuthorizationAsync();

            // find shows on trakt and add + collect them
            var alreadyGotIt = (await client.Sync.GetCollectionShowsAsync()).ToList();
            var exclude = alreadyGotIt.Select(x => x?.Show?.Ids?.Imdb).Where(y => !string.IsNullOrWhiteSpace(y)).ToList();
            Console.WriteLine();
            foreach (var title in alreadyGotIt)
            {
                Console.WriteLine($"Skipping {title.Show.Title} as it's already added.");
            }
            foreach (var imdbId in imdbIds.Keys.Except(exclude))
            {
                Console.Write(imdbIds[imdbId].PadRight(50));
                var shows = await client.Search.GetIdLookupResultsAsync(TraktSearchIdType.ImDB, imdbId, TraktSearchResultType.Show);
                if (shows.ItemCount == 0 || shows.Items.ToList()[0].Show == null)
                {
                    Console.WriteLine("Failed: show not found on trakt!");
                    saveFaileds.Add(imdbIds[imdbId]);
                    continue;
                }
                var show = shows.Items.ToList()[0].Show;
                var thb = new TraktSyncHistoryPostBuilder();
                thb.AddShow(show);
                var r1 = await client.Sync.AddWatchedHistoryItemsAsync(thb.Build());
                var tcb = new TraktSyncCollectionPostBuilder();
                tcb.AddShow(show);
                var r2 = await client.Sync.AddCollectionItemsAsync(tcb.Build());
                Console.WriteLine("Successfully added in the collection and marked as watched!");
            }
            Console.WriteLine();
            foreach (var saveFailed in saveFaileds)
            {
                Console.WriteLine($"Couldn't migrate: {saveFailed}.");
            }
            Console.WriteLine("Done, press any key to quit...");
            Console.ReadLine();
            Debugger.Break();
        }
    }
}
