using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace GetTvShowTotalLength
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please provide a TV show name.");
                Environment.Exit(1);
            }
            string title = args[0];

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync($"https://api.tvmaze.com/search/shows?q={title}"))
                    {
                        string ff = $"https://api.tvmaze.com/search/shows?q={title}";
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Failed.");
                            Environment.Exit(1);
                        }
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var showList = JsonConvert.DeserializeObject<List<dynamic>>(responseBody);


                        if (showList == null || showList.Count == 0)
                        {
                            Environment.Exit(10);
                        }

                        var mostRecentShow = showList
                          .Select(x => x.show)
                          .OrderByDescending(show => DateTime.Parse((string)show.premiered))
                          .FirstOrDefault();

                        if (mostRecentShow == null)
                        {
                            Environment.Exit(10);
                        }

                        string showId = mostRecentShow.id;
                        using (HttpResponseMessage episodesResponse = await client.GetAsync($"https://api.tvmaze.com/shows/{showId}?embed=episodes"))
                        {
                            if (!episodesResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Failed to serch episodes.");
                                Environment.Exit(1);
                            }

                            string episodesResponseBody = await episodesResponse.Content.ReadAsStringAsync();
                            var showData = JsonConvert.DeserializeObject<dynamic>(episodesResponseBody);
                            var episodes = showData?._embedded?.episodes;

                            if (episodes == null)
                            {
                                Console.WriteLine("No episodes found.");
                                Environment.Exit(1);
                            }

                            int totalRuntime = 0;
                            int? runtime = 0;
                            foreach (var episode in episodes)
                            {
                                runtime = episode.runtime;
                                if (runtime.HasValue)
                                {
                                    totalRuntime += runtime.Value;
                                }
                            }

                            Console.WriteLine(totalRuntime);
                        }
                    }
                }          
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Bład - {ex.Message}");
                Environment.Exit(1);
            }
        }

    }
}
