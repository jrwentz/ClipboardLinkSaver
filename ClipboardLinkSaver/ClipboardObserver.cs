using Dapplo.Windows.Clipboard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardLinkSaver
{
    internal class ClipboardObserver : IObserver<ClipboardUpdateInformation>
    {
        private readonly ILogger<ClipboardObserver> _logger;
        private readonly FileHandler _fileHandler;

        internal ClipboardObserver(IHost host, FileHandler fileHandler)
        {
            IHost _host = host ?? throw new ArgumentNullException(nameof(host));
            _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
            _logger = host.Services.GetService<ILogger<ClipboardObserver>>() ?? throw new InvalidOperationException("Could not find logger");

            ClipboardNative.OnUpdate.Skip(1).Where(update => update.FormatIds.Contains((uint)StandardClipboardFormats.UnicodeText)).Publish().RefCount().Subscribe(this);
            _logger.LogDebug("Clipboard Observer initialized");
        }

        public void OnCompleted()
        {
            _logger.LogDebug("Clipboard Observer Complated");
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "ClipboardObserver.OnError()");
        }

        public void OnNext(ClipboardUpdateInformation value)
        {
            _logger.LogTrace("Accessing Clipboard");
            using var clipboard = ClipboardNative.Access();
            var text = clipboard.GetAsUnicodeString();
            _fileHandler.AddItem(text);
        }
    }
}
