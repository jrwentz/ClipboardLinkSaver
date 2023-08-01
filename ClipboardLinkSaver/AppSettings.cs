using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardLinkSaver
{
    internal class AppSettings
    {
        public string File { get; set; } = GenerateSampleFile();
        public HashSet<string> Filters { get; set; } = new HashSet<string>();

        private static string GenerateSampleFile()
        {
            var randomTempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return Path.ChangeExtension(randomTempFile, ".txt");
        }
    }
}
