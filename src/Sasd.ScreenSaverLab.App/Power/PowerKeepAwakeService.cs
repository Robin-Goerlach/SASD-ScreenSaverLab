using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Sasd.ScreenSaverLab.Core;

namespace Sasd.ScreenSaverLab.App;

/// <summary>
/// Uses the official Windows execution-state API to keep the system and optionally
/// the display awake while SASD ScreenSaver Lab is running.
/// </summary>
/// <remarks>
/// This is intentionally implemented without simulated mouse movement. Simulating
/// input would be fragile and intrusive. <c>SetThreadExecutionState</c> is the normal
/// Windows mechanism for applications that need to keep the system awake during an
/// active task such as a presentation, dashboard, visualizer, or kiosk-like display.
/// </remarks>
public sealed class PowerKeepAwakeService : IDisposable
{
    private bool _hasActiveRequest;

    /// <summary>
    /// Applies the requested power-management mode.
    /// </summary>
    /// <param name="mode">The requested keep-awake mode.</param>
    public void Apply(PowerManagementMode mode)
    {
        switch (mode)
        {
            case PowerManagementMode.AllowSleep:
                ClearRequest();
                break;

            case PowerManagementMode.KeepSystemAwake:
                SetRequest(
                    ExecutionState.EsContinuous |
                    ExecutionState.EsSystemRequired);
                break;

            case PowerManagementMode.KeepSystemAndDisplayAwake:
                SetRequest(
                    ExecutionState.EsContinuous |
                    ExecutionState.EsSystemRequired |
                    ExecutionState.EsDisplayRequired);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported power-management mode.");
        }
    }

    /// <summary>
    /// Clears an active keep-awake request when the application exits.
    /// </summary>
    public void Dispose()
    {
        ClearRequest();
    }

    /// <summary>
    /// Sends a continuous execution-state request to Windows.
    /// </summary>
    private void SetRequest(ExecutionState state)
    {
        ExecutionState result = SetThreadExecutionState(state);

        if (result == 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not update the Windows execution state.");
        }

        _hasActiveRequest = true;
    }

    /// <summary>
    /// Clears any active execution-state request created by this service.
    /// </summary>
    private void ClearRequest()
    {
        if (!_hasActiveRequest)
        {
            return;
        }

        // ES_CONTINUOUS without ES_SYSTEM_REQUIRED or ES_DISPLAY_REQUIRED tells Windows
        // to clear the previous continuous requirement set by this thread.
        SetThreadExecutionState(ExecutionState.EsContinuous);
        _hasActiveRequest = false;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    /// <summary>
    /// Flags accepted by the Windows SetThreadExecutionState API.
    /// </summary>
    [Flags]
    private enum ExecutionState : uint
    {
        EsSystemRequired = 0x00000001,
        EsDisplayRequired = 0x00000002,
        EsContinuous = 0x80000000
    }
}
