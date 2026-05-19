using Sasd.ScreenSaverLab.Core;
using Sasd.ScreenSaverLab.Effects;

namespace Sasd.ScreenSaverLab.App;

internal static class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        ScreenSaverStartupOptions options = ScreenSaverCommandLineParser.Parse(args);

        // V0.6.1 deliberately resolves the selected effect a second time through the
        // concrete built-in effect registry. This keeps direct shortcuts such as /lab
        // and /pulse robust even while the command-line parser and effect factory evolve.
        if (BuiltInScreenSaverEffects.TryResolveFromArguments(args, out string canonicalEffectName))
        {
            options = options with { EffectName = canonicalEffectName };
        }
        else
        {
            options = options with { EffectName = BuiltInScreenSaverEffects.ResolveCanonicalName(options.EffectName) };
        }

        if (options.ShowHelp)
        {
            CommandLineHelpText.Show();
            return;
        }

        if (options.ShowEffectList)
        {
            CommandLineHelpText.ShowEffectList();
            return;
        }

        if (options.Mode == ScreenSaverMode.Configure)
        {
            MessageBox.Show(
                $"SASD ScreenSaver Lab V0.6.1\n\nA graphical configuration dialog will be added in a later version.\n\nFor now, use command-line options such as /clock, /no-clock, /effect:star-drift, /effect:data-stream, /effect:light-trails, /effect:amber-feed, /effect:wireframe-terrain, /effect:plasma-grid, /effect:orbit-field, /effect:lab-console or /effect:system-pulse. For Amber Feed, /feeds:config/feeds.json can select a feed configuration file.\n\nRun with /help to show the command-line reference or /list-effects to show supported effect aliases.\n\nSupported effects: {BuiltInScreenSaverEffects.GetSupportedEffectsText()}",
                "SASD ScreenSaver Lab",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (options.Mode == ScreenSaverMode.Preview)
        {
            MessageBox.Show(
                "Preview mode is parsed but not implemented yet.\n\nPlease run the application normally for the V0.6.1 prototype.",
                "SASD ScreenSaver Lab",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        IReadOnlyList<Screen> targetScreens = ScreenSelector.SelectScreens(options);

        using PowerKeepAwakeService powerKeepAwakeService = new();

        try
        {
            powerKeepAwakeService.Apply(options.PowerManagementMode);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"SASD ScreenSaver Lab could not apply the requested power-management mode.\n\nThe screensaver will continue without keep-awake protection.\n\n{ex.Message}",
                "SASD ScreenSaver Lab - Power Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        // Each screen gets its own effect instance. Sharing one effect object between
        // several windows would mix their animation state and screen sizes.
        Application.Run(new ScreenSaverApplicationContext(
            targetScreens,
            effectFactory: () => BuiltInScreenSaverEffects.Create(options.EffectName, options.AmberFeedConfigurationPath),
            clock: new SystemEffectClock(),
            showClockOverlay: options.ShowClockOverlay));
    }
}
