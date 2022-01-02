using System;
using System.IO;
using System.Threading.Tasks;
static class MediaAccess
{
    public static async Task<TResult> WhenMediaMounted<TResult>(Func<Task<TResult>> function) 
    {
        try 
        {
            var affe = await function();
            return affe;
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
            var text = await Process.RunAsync("uhubctl", "-p 5 -a 1 -l 1-1");
            Console.WriteLine($"uhubctl executed {text}");
            try 
            {
                Console.WriteLine("Mounting...");
                // TODO: /media/video/
                text = await Process.RunAsync("mount", "/media/video/");
                Console.WriteLine($"mount executed {text}");
            }
            catch(Exception e)
            {
                Console.WriteLine($"mount error: {e}");
                await Task.Delay(2000);
                // TODO: /media/video/
                text = await Process.RunAsync("mount", "/media/video/");
                Console.WriteLine($"mount executed (2nd time) {text}");
            }
           
            Console.WriteLine("Retrying after mount");
            return await function();
        }
    }
}