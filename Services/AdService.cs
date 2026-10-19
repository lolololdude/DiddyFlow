using DiddyFlow.Helpers;
using DiddyFlow.Models;

namespace DiddyFlow.Services;

public sealed class AdService
{
    private readonly SettingsService _settings;
    private readonly Random _random = new();
    private bool _startupPlayed;

    public AdService(SettingsService settings)
    {
        _settings = settings;
    }

    public static IReadOnlyList<string> Slogans { get; } = new[]
    {
        "THE FLOW IS YOURS.",
        "TAKE THAT, CLOUD.",
        "YOUR WORDS. YOUR MACHINE. YOUR MOMENT.",
        "NOT A SUBSCRIPTION. NOT EVEN CLOSE.",
        "DICTATION WITH BAD BOY ENERGY.",
        "LOCAL. FREE. UNHINGED.",
        "EVERY WORD A STATEMENT.",
        "THE ONLY ADS YOU'LL ACTUALLY ENJOY."
    };

    public bool ShouldPlay()
    {
        var settings = _settings.Settings;
        if (!settings.AdsEnabled)
        {
            return false;
        }

        return settings.AdFrequency switch
        {
            AdFrequency.Startup => !_startupPlayed,
            AdFrequency.AfterTranscription => true,
            AdFrequency.EveryFiveTranscriptions => settings.TranscriptionsSinceAd >= 5,
            AdFrequency.Hourly => settings.LastAdAt is null
                                  || DateTimeOffset.Now - settings.LastAdAt.Value >= TimeSpan.FromHours(1),
            _ => false
        };
    }

    public void MarkPlayed()
    {
        var settings = _settings.Settings;
        settings.TranscriptionsSinceAd = 0;
        settings.LastAdAt = DateTimeOffset.Now;
        _startupPlayed = true;
        _settings.Save();
    }

    public string? NextVideo()
    {
        var videos = EffectiveVideos();
        if (videos.Count == 0)
        {
            return null;
        }

        var index = _random.Next(videos.Count);
        if (videos.Count > 1 && index == _settings.Settings.LastAdIndex)
        {
            index = (index + 1) % videos.Count;
        }

        _settings.Settings.LastAdIndex = index;
        return videos[index];
    }

    public IReadOnlyList<string> EffectiveVideos()
    {
        var result = new List<string>();
        foreach (var video in _settings.Settings.AdVideos)
        {
            if (File.Exists(video) && !result.Contains(video, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(video);
            }
        }

        foreach (var (file, _) in AdFolders.Scan())
        {
            if (!result.Contains(file, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(file);
            }
        }

        return result;
    }

    public IReadOnlyList<(string File, string Source)> Playlist()
    {
        var result = new List<(string, string)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var video in _settings.Settings.AdVideos)
        {
            if (File.Exists(video) && seen.Add(video))
            {
                result.Add((video, "Picked by you"));
            }
        }

        foreach (var (file, folder) in AdFolders.Scan())
        {
            if (seen.Add(file))
            {
                result.Add((file, folder.Label));
            }
        }

        return result;
    }

    public string? RandomSlogan()
    {
        return Slogans[_random.Next(Slogans.Count)];
    }

    public void ResetStartupFlag()
    {
        _startupPlayed = false;
    }
}
