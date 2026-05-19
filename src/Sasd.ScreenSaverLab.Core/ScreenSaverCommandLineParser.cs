namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Parses the small command-line argument set used by Windows screensavers and by developers.
/// </summary>
/// <remarks>
/// Windows commonly starts screensavers with arguments such as <c>/s</c>, <c>/c</c> or
/// <c>/p HWND</c>. This parser also understands a few developer-friendly arguments for
/// multi-monitor testing, for example <c>/screen:1</c>, <c>/primary</c> and
/// <c>/all-screens</c>. V0.1.3 added simple overlay configuration arguments such as
/// <c>/no-clock</c> and <c>/clock:off</c>. V0.2 adds built-in effect selection via
/// <c>/effect:digital-rain</c> or short aliases such as <c>/rain</c>.
/// </remarks>
public static class ScreenSaverCommandLineParser
{
    /// <summary>
    /// Parses command-line arguments into a strongly typed options object.
    /// </summary>
    /// <param name="args">Raw command-line arguments.</param>
    /// <returns>Parsed startup options.</returns>
    public static ScreenSaverStartupOptions Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return ScreenSaverStartupOptions.Default;
        }

        ScreenSaverMode mode = ScreenSaverMode.Normal;
        nint? previewHandle = null;
        int? targetScreenIndex = null;
        bool useMouseScreen = true;
        bool useAllScreens = false;
        bool usePrimaryScreen = false;
        bool showClockOverlay = true;
        string effectName = "star-drift";

        for (int i = 0; i < args.Length; i++)
        {
            string current = NormalizeArgument(args[i]);

            if (current == "s")
            {
                mode = ScreenSaverMode.Fullscreen;

                // When Windows starts the real screensaver, users normally expect every
                // monitor to be covered. Developers can still override this with /screen:N.
                useAllScreens = true;
                usePrimaryScreen = false;
                continue;
            }

            if (current == "c")
            {
                mode = ScreenSaverMode.Configure;
                continue;
            }

            if (current == "p")
            {
                mode = ScreenSaverMode.Preview;

                if (i + 1 < args.Length && nint.TryParse(args[i + 1], out nint handle))
                {
                    previewHandle = handle;
                    i++;
                }

                continue;
            }

            if (current is "all" or "all-screens" or "all-monitors" or "multi")
            {
                useAllScreens = true;
                targetScreenIndex = null;
                useMouseScreen = false;
                usePrimaryScreen = false;
                continue;
            }

            if (current is "primary" or "screen:primary" or "screen=primary")
            {
                targetScreenIndex = null;
                useAllScreens = false;
                useMouseScreen = false;
                usePrimaryScreen = true;
                continue;
            }

            if (current is "mouse" or "mouse-screen" or "screen:mouse" or "screen=mouse")
            {
                targetScreenIndex = null;
                useAllScreens = false;
                useMouseScreen = true;
                usePrimaryScreen = false;
                continue;
            }

            if (TryParseClockOverlayOption(current, out bool parsedShowClockOverlay))
            {
                showClockOverlay = parsedShowClockOverlay;
                continue;
            }

            if (TryParseEffectOption(current, out string parsedEffectName))
            {
                effectName = parsedEffectName;
                continue;
            }

            if (TryParseScreenIndex(current, out int parsedScreenIndex))
            {
                targetScreenIndex = parsedScreenIndex;
                useAllScreens = false;
                useMouseScreen = false;
                usePrimaryScreen = false;
            }
        }

        return new ScreenSaverStartupOptions(
            mode,
            previewHandle,
            targetScreenIndex,
            useMouseScreen,
            useAllScreens,
            usePrimaryScreen,
            showClockOverlay,
            effectName);
    }

    /// <summary>
    /// Normalizes arguments so <c>/s</c>, <c>-s</c> and <c>s</c> can be handled uniformly.
    /// </summary>
    private static string NormalizeArgument(string argument)
    {
        return argument
            .Trim()
            .TrimStart('/', '-')
            .ToLowerInvariant();
    }

    /// <summary>
    /// Parses supported clock overlay argument formats.
    /// </summary>
    private static bool TryParseClockOverlayOption(string argument, out bool showClockOverlay)
    {
        showClockOverlay = true;

        if (argument is "clock" or "show-clock" or "with-clock")
        {
            showClockOverlay = true;
            return true;
        }

        if (argument is "no-clock" or "hide-clock" or "without-clock")
        {
            showClockOverlay = false;
            return true;
        }

        string[] enabledValues = ["clock:on", "clock=on", "clock:true", "clock=true", "showclock:on", "showclock=true"];
        string[] disabledValues = ["clock:off", "clock=off", "clock:false", "clock=false", "showclock:off", "showclock=false"];

        if (enabledValues.Contains(argument, StringComparer.OrdinalIgnoreCase))
        {
            showClockOverlay = true;
            return true;
        }

        if (disabledValues.Contains(argument, StringComparer.OrdinalIgnoreCase))
        {
            showClockOverlay = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Parses supported effect argument formats.
    /// </summary>
    /// <remarks>
    /// The parser deliberately accepts friendly aliases, but it does not instantiate any
    /// effect. Instantiation stays in the application/effects layer so the core project
    /// does not need to reference the concrete effect implementations.
    /// </remarks>
    private static bool TryParseEffectOption(string argument, out string effectName)
    {
        effectName = "star-drift";

        if (argument is "star" or "stars" or "star-drift" or "effect:star" or "effect=star" or "effect:star-drift" or "effect=star-drift")
        {
            effectName = "star-drift";
            return true;
        }

        if (argument is "rain" or "digital-rain" or "code-rain" or "matrix" or "effect:rain" or "effect=rain" or "effect:digital-rain" or "effect=digital-rain" or "effect:code-rain" or "effect=code-rain")
        {
            effectName = "digital-rain";
            return true;
        }

        string[] prefixes = ["effect:", "effect=", "visual:", "visual="];

        foreach (string prefix in prefixes)
        {
            if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                string value = argument[prefix.Length..].Trim();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    effectName = value;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Parses supported screen-index argument formats.
    /// </summary>
    private static bool TryParseScreenIndex(string argument, out int screenIndex)
    {
        screenIndex = 0;

        string[] prefixes =
        [
            "screen:",
            "screen=",
            "monitor:",
            "monitor=",
            "display:",
            "display="
        ];

        foreach (string prefix in prefixes)
        {
            if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                string value = argument[prefix.Length..];
                return int.TryParse(value, out screenIndex) && screenIndex >= 0;
            }
        }

        return false;
    }
}
