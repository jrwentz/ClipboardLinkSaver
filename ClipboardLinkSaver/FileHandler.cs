using Dapplo.Windows.Clipboard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Joins;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace ClipboardLinkSaver
{
    internal class FileHandler
    {
        private readonly ILogger<FileHandler> _logger;
        private readonly string _file;
        private readonly OrderedSet<string> _items;
        private readonly bool _hasFilters = false;
        private readonly Regex _regex = new("");

        internal FileHandler(IHost host)
        {
            var _ = host ?? throw new ArgumentNullException(nameof(host));
            _logger = host.Services.GetService<ILogger<FileHandler>>() ?? throw new InvalidOperationException("Could not find logger");
            _items = new OrderedSet<string>(StringComparer.CurrentCultureIgnoreCase);

            var settings = host.Services.GetService<AppSettings>() ?? throw new InvalidOperationException("Could not find settings");
            _file = settings.File;
            _logger.LogInformation($"File: {_file}");
            var urlFilters = settings.Filters;

            if (urlFilters != null && urlFilters.Count > 0)
            {
                _logger.LogInformation($"Filters:\n  {string.Join("\n  ", urlFilters)}");
                _logger.LogTrace($"{urlFilters.Count} Filters found");
                _hasFilters = true;
                _regex = SetupFilters(urlFilters);
            }
            else
            {
                _logger.LogInformation("No Filters found");
            }

            LoadCurrentFile();

            _logger.LogDebug("File Handler initialized");
        }

        private Regex SetupFilters(HashSet<string> filters)
        {
            var pattern = $"({string.Join('|', filters)})";
            _logger.LogTrace($"Compiling Filter Regex: {pattern}");
            return new Regex(pattern, RegexOptions.Compiled);
        }

        private void LoadCurrentFile()
        {
            try
            {
                if (File.Exists(_file))
                {
                    _logger.LogDebug($"Reading {_file}...");
                    var currentContents = File.ReadAllLines(_file);
                    currentContents.Where(line => !string.IsNullOrWhiteSpace(line)).ToList().ForEach(line => _items.Add(line));
                    _logger.LogInformation($"Loaded {_items.Count} lines from {_file}");
                }
                else
                {
                    _logger.LogWarning($"Could not find file {_file}; it will be created on save or exit.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trying to load {_filePath}", _file);
            }
        }

        internal void SaveFile()
        {
            try
            {
                File.WriteAllLines(_file, _items);
                _logger.LogInformation($"Saved list to {_file}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Trying to save {_file}");
            }
        }

        internal void ReloadFile()
        {
            try
            {
                _logger.LogDebug($"Clearing list");
                _items.Clear();
                if (File.Exists(_file))
                {
                    _logger.LogDebug($"Found {_file}, opening...");
                    var currentContents = File.ReadAllLines(_file);
                    currentContents.Where(line => !string.IsNullOrWhiteSpace(line)).ToList().ForEach(line => _items.Add(line));
                    _logger.LogInformation($"Reloaded {_items.Count} items from {_file}");
                }
                else
                {
                    _logger.LogWarning($"Could not find file {_file}; it will be created on save or exit.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Trying to load {_file}");
            }
        }

        internal void AddItem(string item)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                item = item.Trim();
                var items = item.Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in items)
                {
                    if (_hasFilters)
                    {
                        _logger.LogTrace($"Checking against Filters: {line}");
                        if (!_regex.IsMatch(line))
                        {
                            _logger.LogDebug($"Item does not match a filter, skipping: {line}");
                            continue;
                        }
                    }

                    _logger.LogTrace($"Adding: {line}");
                    var isNew = _items.Add(line);
                    if (isNew)
                    {
                        _logger.LogInformation($"Added {line}");
                    }
                    else
                    {
                        _logger.LogDebug($"Skipped {line}");
                    }
                }
            }
            else
            {
                _logger.LogTrace("Item was null or empty");
            }
        }
    }
}
