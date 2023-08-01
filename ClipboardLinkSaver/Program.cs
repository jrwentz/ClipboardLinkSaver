
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ClipboardLinkSaver
{
    internal class Program
    {
        private static bool _stopMe = false;
        private static ILogger<Program> _logger;

        [STAThread]
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args) //new HostBuilder()
                .ConfigureCoreConfig()
                .ConfigureCoreLogging()
                .Build();

            host.Start();

            _logger = host.Services.GetRequiredService<ILogger<Program>>();

            var fileHandler = new FileHandler(host);
            var clipboardObserver = new ClipboardObserver(host, fileHandler);
            Console.CancelKeyPress += Console_CancelKeyPress;

            PrintHelp();

            // Create an observable instance
            var observable = ObserveKeysUntilEscape();

            // Subscribe to the observable sequence
            observable.Subscribe(
                keyInfo =>
                {
                    // Do something with the keystroke
                    _logger.LogTrace("Key pressed: {0}", keyInfo.Key);
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.S:
                            fileHandler.SaveFile(); 
                            break;
                        case ConsoleKey.R:
                            fileHandler.ReloadFile();
                            break;
                        default:
                            PrintHelp();
                            break;
                    }
                },
                () =>
                {
                    _stopMe = true;
                }
            );


            while (!_stopMe)
            {
                Application.DoEvents();
            }

            fileHandler.SaveFile();
            _logger.LogInformation("Exiting!");
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _logger.LogDebug("Stopping Application");
            _stopMe = true;
        }

        private static IObservable<ConsoleKeyInfo> ObserveKeysUntilEscape()
        {
            var keys = new Subject<ConsoleKeyInfo>();

            Task.Run(
                () =>
                {
                    ConsoleKeyInfo key;
                    do
                    {
                        key = Console.ReadKey(true);
                        keys.OnNext(key);
                    } while (key.Key != ConsoleKey.Escape);
                    keys.OnCompleted();
                });

            return keys;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Press S to Save, R to Reload, Esc to exit.");
        }
    }
}