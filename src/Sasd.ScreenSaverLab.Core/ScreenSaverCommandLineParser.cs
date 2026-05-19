namespace Sasd.ScreenSaverLab.Core;

/// <summary>
/// Parses the small command-line argument set used by Windows screensavers and by developers.
/// </summary>
/// <remarks>
/// Windows commonly starts screensavers with arguments such as <c>/s</c>, <c>/c</c> or
/// <c>/p HWND</c>. This parser also understands a few developer-friendly arguments for
/// multi-monitor testing, for example <c>/screen:1</c>, <c>/primary</c> and
/// <c>/all-screens</c>.
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
            usePrimaryScreen);
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
