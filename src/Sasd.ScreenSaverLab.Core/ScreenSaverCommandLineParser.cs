namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Parses the small command-line argument set used by Windows screensavers and by developers.
/// </summary>
/// <remarks>
/// Windows commonly starts screensavers with arguments such as <c>/s</c>, <c>/c</c> or
/// <c>/p HWND</c>. This parser also understands developer-friendly arguments for
/// multi-monitor testing, for example <c>/screen:1</c>, <c>/primary</c> and
/// <c>/all-screens</c>. It also supports overlay configuration, built-in effect selection,
/// optional power-management behavior and a manpage-like <c>/help</c> output.
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
        bool showHelp = false;
        PowerManagementMode powerManagementMode = PowerManagementMode.AllowSleep;

        for (int i = 0; i < args.Length; i++)
        {
            string current = NormalizeArgument(args[i]);

            if (IsHelpArgument(current))
            {
                showHelp = true;
                continue;
            }

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

            if (TryParsePowerManagementOption(current, out PowerManagementMode parsedPowerManagementMode))
            {
                powerManagementMode = parsedPowerManagementMode;
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
            effectName,
            showHelp,
            powerManagementMode);
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
    /// Detects arguments that should show the command-line help.
    /// </summary>
    private static bool IsHelpArgument(string argument)
    {
        return argument is "help" or "?" or "h" or "man" or "usage";
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

        if (argument is "data" or "stream" or "datastream" or "data-stream" or "cipher" or "cipherfall"
            or "effect:data" or "effect=data"
            or "effect:stream" or "effect=stream"
            or "effect:datastream" or "effect=datastream"
            or "effect:data-stream" or "effect=data-stream"
            or "effect:cipherfall" or "effect=cipherfall")
        {
            effectName = "data-stream";
            return true;
        }

        if (argument is "light" or "lights" or "trail" or "trails" or "light-trail" or "light-trails" or "lighttrail"
            or "effect:light" or "effect=light"
            or "effect:lights" or "effect=lights"
            or "effect:trail" or "effect=trail"
            or "effect:trails" or "effect=trails"
            or "effect:light-trail" or "effect=light-trail"
            or "effect:light-trails" or "effect=light-trails"
            or "effect:lighttrail" or "effect=lighttrail")
        {
            effectName = "light-trails";
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
    /// Parses supported Windows power-management argument formats.
    /// </summary>
    /// <remarks>
    /// The default intentionally remains <see cref="PowerManagementMode.AllowSleep" />.
    /// Keep-awake behavior is opt-in because a screensaver should not silently override
    /// the user's Windows power plan.
    /// </remarks>
    private static bool TryParsePowerManagementOption(string argument, out PowerManagementMode mode)
    {
        mode = PowerManagementMode.AllowSleep;

        if (argument is "allow-sleep" or "sleep" or "respect-power-plan" or "power:allow-sleep" or "power=allow-sleep")
        {
            mode = PowerManagementMode.AllowSleep;
            return true;
        }

        if (argument is "keep-awake" or "keep-system-awake" or "stay-awake" or "nosleep" or "no-sleep"
            or "power:keep-awake" or "power=keep-awake" or "power:system" or "power=system")
        {
            mode = PowerManagementMode.KeepSystemAwake;
            return true;
        }

        if (argument is "keep-display-awake" or "keep-screen-awake" or "display-awake" or "screen-awake"
            or "power:display" or "power=display" or "power:keep-display-awake" or "power=keep-display-awake")
        {
            mode = PowerManagementMode.KeepSystemAndDisplayAwake;
            return true;
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
