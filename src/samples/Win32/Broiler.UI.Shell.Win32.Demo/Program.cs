using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Broiler.Native.Windows;

namespace Broiler.UI.Shell.Win32.Demo;

[SupportedOSPlatform("windows7.0")]
internal static class Program
{
    [STAThread]
    private static int Main()
    {
        _ = WindowNative.SetProcessDpiAwarenessContext(new IntPtr(-4));

        try
        {
            using var window = new ShellDemoWindow();
            return window.Run();
        }
        catch (Exception ex)
        {
            WindowNative.MessageBox(IntPtr.Zero, ex.ToString(), "Broiler.UI.Shell Win32 Demo", MbIconError | MbOk);
            return 1;
        }
    }

    private const uint MbOk = 0x00000000;
    private const uint MbIconError = 0x00000010;
}
