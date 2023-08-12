using static CsTools.ProcessCmd;
using static System.Console;
using static Configuration;
using LinqTools;

static class DiskAccess
{
    public static async Task AccessDisk(HttpContext context)
    {
        WriteLine("Accessing disk");
        var result = await RunAsync("/usr/sbin/uhubctl", $"-l 1-1 -a 1 -p {GetEnvironmentVariable(UsbMediaPort)}");
        await Task.Delay(6000);
        var mountResult = await RunAsync("/usr/bin/mount", "-a");
        await context.Response.WriteAsync("Disk accessed");
        if (diskShutdownTimer.Enabled)
            diskShutdownTimer.Stop();
        diskShutdownTimer.Start();
        WriteLine(result);
        WriteLine(mountResult);
    }

    public static Task DiskNeeded(HttpContext context)
        => context
            .SideEffect(_ => 
            {
                WriteLine("Disk needed");
                if (diskShutdownTimer.Enabled)
                    diskShutdownTimer.Stop();
                diskShutdownTimer.Start();
            })
            .Response
            .WriteAsync("Disk shutdown delayed");

    static DiskAccess()
        => diskShutdownTimer = new System.Timers.Timer(300_000)
        {
            AutoReset = false,
        }.SideEffect(t => t.Elapsed += async (s, e) =>
        {
            WriteLine("Switching disk off...");
            var result = await RunAsync("/usr/sbin/uhubctl", $"-l 1-1 -a 0 -p {GetEnvironmentVariable(UsbMediaPort)} -r 500");
            WriteLine("disk switched off");
            WriteLine(result);
        });

    static System.Timers.Timer diskShutdownTimer;
}