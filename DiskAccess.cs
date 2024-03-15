using static CsTools.ProcessCmd;
using static System.Console;
using static Configuration;
using static CsTools.Core;
using CsTools.Extensions;

static class DiskAccess
{
    public static async Task AccessDisk(HttpContext context)
    {
        await AccessDisk();
        await context.Response.WriteAsync("Disk accessed");
    }

    public static Task DiskNeeded(HttpContext context)
        => context
            .SideEffect(_ => 
            {
                WriteLine("Disk needed");
                if (diskShutdownTimer.Enabled) {
                    diskShutdownTimer.Stop();
                    diskShutdownTimer.Start();
                } 
                else
                    StartAccessDisk();
            })
            .Response
            .WriteAsync("Disk shutdown delayed");

    static async Task AccessDisk()
    {
        WriteLine("Accessing disk");
        var result = await Try(() =>  RunAsync("/usr/sbin/uhubctl", $"-l 1-1 -a 1 -p {GetEnvironmentVariable(UsbMediaPort)}"), e => $"{e}");
        await Task.Delay(6000);
        var mountResult = await Try(() =>  RunAsync("/usr/bin/mount", "-a"), e => $"{e}");
        if (diskShutdownTimer.Enabled)
            diskShutdownTimer.Stop();
        diskShutdownTimer.Start();
        WriteLine(result);
        WriteLine(mountResult);
    }

    static async void StartAccessDisk()
    {
        try 
        {
            await AccessDisk();
        }
        catch {}
    }
    static DiskAccess()
        => diskShutdownTimer = new System.Timers.Timer(300_000)
        {
            AutoReset = false,
        }.SideEffect(t => t.Elapsed += async (s, e) =>
        {
            WriteLine("Switching disk off...");
            var result = await Try(() =>  RunAsync("/usr/sbin/uhubctl", $"-l 1-1 -a 0 -p {GetEnvironmentVariable(UsbMediaPort)} -r 500"), e => $"{e}");
            WriteLine("disk switched off");
            WriteLine(result);
            diskShutdownTimer?.Stop();
        });

    static System.Timers.Timer diskShutdownTimer;
}