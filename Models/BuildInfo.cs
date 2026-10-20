namespace DiddyFlow.Models;

public enum BuildEdition
{
    Consumer,
    Normal,
    Developer
}

public static class BuildInfo
{
#if DIDDY_DEVELOPER
    public const BuildEdition Edition = BuildEdition.Developer;
#elif DIDDY_NORMAL
    public const BuildEdition Edition = BuildEdition.Normal;
#else
    public const BuildEdition Edition = BuildEdition.Consumer;
#endif

    public static bool HasAdsTab { get; } =
#if DIDDY_ADS_TAB
        true;
#else
        false;
#endif

    public static bool HasAdsSettings { get; } =
#if DIDDY_ADS_SETTINGS
        true;
#else
        false;
#endif

    public static bool HasAdvanced { get; } =
#if DIDDY_ADVANCED
        true;
#else
        false;
#endif

    public static bool HasAllAdFolders { get; } =
#if DIDDY_ALL_AD_FOLDERS
        true;
#else
        false;
#endif

    public static bool IsDeveloper => Edition == BuildEdition.Developer;

    public static string EditionName => Edition.ToString();
}
