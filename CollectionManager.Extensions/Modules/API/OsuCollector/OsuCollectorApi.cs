namespace CollectionManager.Extensions.Modules.API.OsuCollector;

using CollectionManager.Core.Modules.FileIo.OsuDb;
using CollectionManager.Core.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Client for osucollector.com: fetches collection metadata by collection ID.
/// Mirrors the endpoint used by the osu-collect tool
/// (<c>GET https://osucollector.com/api/collections/{id}</c>).
/// </summary>
public class OsuCollectorApi
{
    public const string BaseUrl = "https://osucollector.com/api/collections";
    public const string CollectionUrlPrefix = "https://osucollector.com/collections/";

    private readonly HttpClient _httpClient;

    public OsuCollectorApi()
        : this(new HttpClient { Timeout = TimeSpan.FromSeconds(15) })
    {
    }

    internal OsuCollectorApi(HttpClient httpClient) => _httpClient = httpClient;

    /// <summary>
    /// Parses a collection ID or a <c>https://osucollector.com/collections/{id}</c> URL.
    /// Accepted: bare numeric ID, URL with or without a trailing slash. Anything
    /// else (other host, scheme, path shape) throws <see cref="ArgumentException"/>.
    /// </summary>
    public static int ParseCollectionId(string input)
    {
        string trimmed = input?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            throw new ArgumentException("Collection ID or URL cannot be empty.");
        }

        if (trimmed.All(char.IsAsciiDigit))
        {
            if (int.TryParse(trimmed, out int parsedId) && parsedId > 0)
            {
                return parsedId;
            }

            throw new ArgumentException($"Invalid collection ID: {trimmed}");
        }

        if (!trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Collection URL must be HTTPS: " + trimmed);
        }

        string afterScheme = trimmed.Substring("https://".Length);
        int firstSlash = afterScheme.IndexOf('/');
        string host = firstSlash < 0 ? afterScheme : afterScheme[..firstSlash];

        if (!host.Equals("osucollector.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Collection URL must be on osucollector.com: {trimmed}");
        }

        string path = firstSlash < 0 ? string.Empty : afterScheme[firstSlash..];
        string prefix = "/collections/";
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid collection URL: {trimmed}");
        }

        string idString = path[prefix.Length..].TrimEnd('/');
        if (string.IsNullOrEmpty(idString) || idString.Contains('/'))
        {
            throw new ArgumentException($"Invalid collection URL: {trimmed}");
        }

        if (int.TryParse(idString, out int id) && id > 0)
        {
            return id;
        }

        throw new ArgumentException($"Collection ID must be numeric: {idString}");
    }

    /// <summary>
    /// Fetches collection data from osucollector.com.
    /// </summary>
    /// <exception cref="HttpRequestException">Network failure or non-success status.</exception>
    public async Task<OsuCollectorCollection> GetCollectionAsync(int collectionId)
    {
        string json = await _httpClient.GetStringAsync($"{BaseUrl}/{collectionId}");
        OsuCollectorCollection collection = JsonConvert.DeserializeObject<OsuCollectorCollection>(json);

        if (collection == null || string.IsNullOrWhiteSpace(collection.Name))
        {
            throw new InvalidOperationException($"osucollector.com did not return a valid collection for ID {collectionId}.");
        }

        return collection;
    }

    /// <summary>
    /// Converts fetched data into a Collection Manager collection. Beatmaps carry
    /// their real MD5 checksum, beatmap ID and beatmapset ID, so maps missing
    /// locally automatically land in <see cref="IOsuCollection.DownloadableBeatmaps"/>
    /// and can be downloaded with the existing download flow.
    /// </summary>
    public OsuCollection ToOsuCollection(OsuCollectorCollection collection, MapCacher mapCacher)
    {
        OsuCollection osuCollection = new(mapCacher)
        {
            Name = collection.Name,
            OnlineId = collection.Id
        };

        foreach (OsuCollectorBeatmapset beatmapset in collection.Beatmapsets)
        {
            foreach (OsuCollectorBeatmap beatmap in beatmapset.Beatmaps)
            {
                osuCollection.AddBeatmap(new BeatmapExtension
                {
                    Md5 = beatmap.Checksum,
                    MapId = beatmap.Id,
                    MapSetId = beatmapset.Id
                });
            }
        }

        return osuCollection;
    }

    /// <summary>
    /// Checks whether any loaded collection already has this osucollector ID.
    /// </summary>
    public static bool IsAlreadyImported(int onlineId, OsuCollections loadedCollections)
        => loadedCollections.Any(c => c.OnlineId == onlineId);
}

/// <summary>Full collection data returned by the osucollector.com API.</summary>
public class OsuCollectorCollection
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public OsuCollectorUploader Uploader { get; set; }

    public List<OsuCollectorBeatmapset> Beatmapsets { get; set; } = [];

    public int Favourites { get; set; }

    [JsonIgnore]
    public int BeatmapCount => Beatmapsets?.Sum(b => b.Beatmaps.Count) ?? 0;
}

public class OsuCollectorUploader
{
    public int Id { get; set; }

    public string Username { get; set; }
}

public class OsuCollectorBeatmapset
{
    public int Id { get; set; }

    public List<OsuCollectorBeatmap> Beatmaps { get; set; } = [];
}

public class OsuCollectorBeatmap
{
    public int Id { get; set; }

    public string Checksum { get; set; }
}