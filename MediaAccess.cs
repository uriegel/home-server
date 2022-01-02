using System;
using System.IO;
using System.Threading.Tasks;
static class MediaAccess
{
    public static async Task<TResult> WhenMediaMounted<TResult>(int usbMediaPort, string mountPath, Func<Task<TResult>> function) 
    {
        try 
        {
            return await function();
        }
        catch (DirectoryNotFoundException)
        {
            return await WhenError();
        }
        catch (IOException)
        {
            return await WhenError();
        }

        async Task<TResult> WhenError() 
        {
            var text = await Process.RunAsync("uhubctl", $"-p {usbMediaPort} -a 1 -l 1-1");
            Console.WriteLine($"uhubctl executed {text}");
            try 
            {
                Console.WriteLine("Mounting...");
                text = await Process.RunAsync("mount", mountPath);
                Console.WriteLine($"mount executed {text}");
            }
            catch(Exception e)
            {
                Console.WriteLine($"mount error: {e}");
                await Task.Delay(2000);
                text = await Process.RunAsync("mount", mountPath);
                Console.WriteLine($"mount executed (2nd time) {text}");
            }
           
            Console.WriteLine("Retrying after mount");
            return await function();
        }
    }
}