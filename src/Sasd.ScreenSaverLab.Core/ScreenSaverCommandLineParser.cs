namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Parses the small command-line argument set used by Windows screensavers and by developers.
/// </summary>
/// <remarks>
/// Windows commonly starts screensavers with arguments such as <c>/s</c>, <c>/c</c> or
/// <c>/p HWND</c>. This parser also understands a few developer-friendly arguments for
/// multi-monitor testing, for example <c>/screen:1</c>, <c>/primary</c> and
/// <c>/all-screens</c>. V0.1.3 adds simple overlay configuration arguments such as
/// <c>/no-clock</c> and <c>/clock:off</c>.
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
            showClockOverlay);
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
    /// <remarks>
    /// Supported examples:
    /// <list type="bullet">
    /// <item><description><c>/clock</c>, <c>/show-clock</c>, <c>/clock:on</c>, <c>/clock=true</c></description></item>
    /// <item><description><c>/no-clock</c>, <c>/hide-clock</c>, <c>/clock:off</c>, <c>/clock=false</c></description></item>
    /// </list>
    /// </remarks>
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
    /// Parses supported screen-index argument formats.
    /// </summary>
    /// <remarks>
    /// Supported examples: <c>/screen:1</c>, <c>/screen=1</c>, <c>/monitor:1</c>,
    /// <c>/display=1</c>. The index is zero-based.
    /// </remarks>
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
