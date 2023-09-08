using MultiLyricsProviderInterface;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using PreferenceManagerLibrary.Preferences;
using System.Threading;
using System.Threading.Tasks;
using System.Resources;

namespace VocaDBLyricsProviderPlugin
{
    public class VocaDBLyricsProvider : LocalisationProviderBase, IMultiLyricsProvider
    {
        public string Name => "VocaDB";
        public PreferenceCollection Preferences
        {
            get
            {
                if (preferences == null)
                {
                    var locKey = $"%{Name}%";
                    var prefTab = new PreferenceCollection($"{locKey}.name", $"{locKey}.name", Name);
                    var general = new PreferenceCollection($"{locKey}.general", $"{locKey}.general", $"{locKey}.descritpion");

                    prefTab.ChildrenPreferences.Add(general);

                    preferences = prefTab;
                }
                return preferences;
            }
        }

        private PreferenceCollection preferences;
        private readonly RestClient client;

        public VocaDBLyricsProvider()
        {
            client = new RestClient(new RestClientOptions("https://vocadb.net/api/") { MaxTimeout = 10000 });
        }
        public async Task<IEnumerable<FoundTrack>> FindLyricsAsync(string album, string title, string artist, CancellationToken token)
        {
            var request = new RestRequest("songs");
            request.AddParameter("query", $"{title}");
            request.AddParameter("lang", "Romaji");
            request.AddParameter("fields", "AdditionalNames,Lyrics");

            var response = await client.GetAsync<VocaDbSongsResponse>(request, token);

            return SongsToFoundTracks(response.items);
        }
        private IEnumerable<FoundTrack> SongsToFoundTracks(IEnumerable<VocaDbSong> songs)
        {
            foreach (var track in songs)
            {
                foreach (var lyrics in track.Lyrics)
                {
                    var dict = new Dictionary<string, string>
                    {
                        { "Id", track.Id.ToString() },
                        { "SongType", track.SongType },
                        { "CultureCode", lyrics.CultureCode },
                        { "TranslationType", lyrics.TranslationType },
                        { "Title", track.Name },
                        { "AdditionalTitles", track.AdditionalNames },
                        { "Producer", track.Producer },
                        { "Length", track.LengthString },
                        { "Release", track.ReleaseDateString }
                    };

                    var fixedLyrics = lyrics.Value;

                    if (fixedLyrics.Contains("\n\n\n"))
                        fixedLyrics = fixedLyrics.Replace("\n\n", "\n");
                    if (fixedLyrics.Contains("\r\n\r\n\r\n"))
                        fixedLyrics = fixedLyrics.Replace("\r\n\r\n", "\r\n");

                    yield return new FoundTrack(dict, fixedLyrics);
                }
            }
        }
        private class VocaDbSongsResponse
        {
            public VocaDbSong[] items { get; set; }
        }
        private class VocaDbSong
        {
            [JsonPropertyName("additionalNames")]
            public string AdditionalNames { get; set; }

            [JsonPropertyName("defaultName")]
            public string Name { get; set; }

            [JsonPropertyName("artistString")]
            public string Producer { get; set; }

            [JsonPropertyName("songType")]
            public string SongType { get; set; }

            [JsonPropertyName("publishDate")]
            public DateTime ReleaseDate { private get; set; }
            public string ReleaseDateString => ReleaseDate.ToString("d");

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("lengthSeconds")]
            public int LengthSeconds { private get; set; }
            public string LengthString => TimeSpan.FromSeconds(LengthSeconds).ToString(@"m\:ss");


            [JsonPropertyName("lyrics")]
            public VocaDBLyrics[] Lyrics { get; set; }
        }
        private class VocaDBLyrics
        {
            [JsonPropertyName("cultureCode")]
            public string CultureCode { get; set; }
            [JsonPropertyName("translationType")]
            public string TranslationType { get; set; }
            [JsonPropertyName("value")]
            public string Value { get; set; }
        }
    }

    public class CosturaInitialization
    {
        public CosturaInitialization()
        {
            CosturaUtility.Initialize();
        }
    }
}
