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

        if (options.ShowHelp)
        {
            CommandLineHelpText.Show();
            return;
        }

        if (options.Mode == ScreenSaverMode.Configure)
        {
            MessageBox.Show(
                $"SASD ScreenSaver Lab V0.2.1\n\nA graphical configuration dialog will be added in a later version.\n\nFor now, use command-line options such as /clock, /no-clock, /effect:star-drift or /effect:data-stream.\n\nRun with /help to show the command-line reference.\n\nSupported effects: {BuiltInScreenSaverEffects.GetSupportedEffectsText()}",
                "SASD ScreenSaver Lab",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (options.Mode == ScreenSaverMode.Preview)
        {
            MessageBox.Show(
                "Preview mode is parsed but not implemented yet.\n\nPlease run the application normally for the V0.2.1 prototype.",
                "SASD ScreenSaver Lab",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        IReadOnlyList<Screen> targetScreens = ScreenSelector.SelectScreens(options);

        // Each screen gets its own effect instance. Sharing one effect object between
        // several windows would mix their animation state and screen sizes.
        Application.Run(new ScreenSaverApplicationContext(
            targetScreens,
            effectFactory: () => BuiltInScreenSaverEffects.Create(options.EffectName),
            clock: new SystemEffectClock(),
            showClockOverlay: options.ShowClockOverlay));
    }
}
