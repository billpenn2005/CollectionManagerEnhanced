namespace CollectionManager.Core.Modules.FileIo;

using CollectionManager.Core.Enums;
using CollectionManager.Core.Types;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Lightweight .osu file parser used to register freshly downloaded beatmap sets into
/// <see cref="MapCacher"/> so collections stop showing them as missing.
/// </summary>
public static class BeatmapFileParser
{
    /// <summary>
    /// Parses the sections used by the beatmap listing (Metadata/General/Difficulty) and computes
    /// the osu! legacy beatmap hash (MD5 of the file name with spaces removed).
    /// </summary>
    public static BeatmapExtension Parse(string osuFilePath)
    {
        BeatmapExtension beatmap = new()
        {
            OsuFileName = Path.GetFileName(osuFilePath),
            Dir = Path.GetFileName(Path.GetDirectoryName(osuFilePath))
        };

        string section = null;
        float circleSize = 5f;
        float approachRate = 5f;
        float overallDifficulty = 5f;
        using StreamReader reader = new(osuFilePath);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (trimmed[0] == '[')
            {
                section = trimmed;
                continue;
            }

            int separator = trimmed.IndexOf(':');
            if (separator <= 0 || separator == trimmed.Length - 1)
            {
                continue;
            }

            string key = trimmed[..separator].Trim();
            string value = trimmed[(separator + 1)..].Trim();

            switch (section)
            {
                case "[General]":
                    if (key == "Mode" && int.TryParse(value, out int mode))
                    {
                        beatmap.PlayMode = (PlayMode)mode;
                    }

                    break;
                case "[Metadata]":
                    switch (key)
                    {
                        case "Title":
                            beatmap.TitleRoman = value;
                            break;
                        case "TitleUnicode":
                            beatmap.TitleUnicode = value;
                            break;
                        case "Artist":
                            beatmap.ArtistRoman = value;
                            break;
                        case "ArtistUnicode":
                            beatmap.ArtistUnicode = value;
                            break;
                        case "Creator":
                            beatmap.Creator = value;
                            break;
                        case "Version":
                            beatmap.DiffName = value;
                            break;
                        case "BeatmapID":
                            _ = int.TryParse(value, out int mapId);
                            beatmap.MapId = mapId;
                            break;
                        case "BeatmapSetID":
                            _ = int.TryParse(value, out int mapSetId);
                            beatmap.MapSetId = mapSetId;
                            break;
                    }

                    break;
                case "[Difficulty]":
                    switch (key)
                    {
                        case "CircleSize":
                            _ = float.TryParse(value, out circleSize);
                            break;
                        case "ApproachRate":
                            _ = float.TryParse(value, out approachRate);
                            break;
                        case "OverallDifficulty":
                            _ = float.TryParse(value, out overallDifficulty);
                            break;
                    }

                    break;
            }
        }

        beatmap.CircleSize = circleSize;
        beatmap.ApproachRate = approachRate;
        beatmap.OverallDifficulty = overallDifficulty;
        beatmap.Md5 = ComputeOsuLegacyHash(osuFilePath);
        return beatmap;
    }

    /// <summary>osu! legacy beatmap hash: MD5 of the .osu file name with all spaces removed.</summary>
    private static string ComputeOsuLegacyHash(string osuFilePath)
    {
        string fileName = Path.GetFileName(osuFilePath);
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(fileName.Replace(" ", string.Empty)));
        return Convert.ToHexString(hash);
    }
}