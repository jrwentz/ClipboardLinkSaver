using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Attributes;
using NLog.Conditions;
using NLog.Extensions.Logging;
using NLog.Filters;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardLinkSaver
{
    public static class ApplicationHostExtensions
    {
        public static IHostBuilder ConfigureCoreConfig(this IHostBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ConfigureServices((builderContext, services) =>
            {
                services.AddSingleton(builderContext.Configuration.GetSection("Settings").Get<AppSettings>() ?? new AppSettings());
                services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
            });

            return builder;
        }

        public static IHostBuilder ConfigureCoreLogging(this IHostBuilder builder)
        {
            return builder.ConfigureLogging((builderContext, loggingBuilder) =>
            {
                var logLevel = builderContext.Configuration.GetValue("Logging:LogLevel:Default", LogLevel.Information);
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(logLevel);
                loggingBuilder.AddNLog(builderContext.Configuration);
                ConfigureNLog(logLevel);
            });
        }

        private static void ConfigureNLog(LogLevel logLevel)
        {
            var nConfig = new NLog.Config.LoggingConfiguration();

            var consoleMinLevel = LogLevel.Information < logLevel ? NLog.LogLevel.FromOrdinal((int)logLevel) : NLog.LogLevel.Info;

            //File target for Captured Links
            var filterTarget = new FilteringTargetWrapper()
            {
                Name = "links_filter",
                Filter = new ConditionBasedFilter()
                {
                    Action = FilterResult.Ignore,
                    Condition = ConditionParser.ParseExpression("not starts-with(message, 'http')")
                },
                //Condition = ConditionParser.ParseExpression("starts-with(message, 'http')"),
                WrappedTarget = new FileTarget("logfile_links")
                {
                    FileName = $"{Application.StartupPath}/${{processname}}_links_${{shortdate}}.log",
                    Layout = @"${message}"
                }
            };
            nConfig.AddTarget(filterTarget);
            nConfig.AddRuleForOneLevel(NLog.LogLevel.Trace, filterTarget);

            //Console Target
            var consoleTarget = new ColoredConsoleTarget("logconsole")
            {
                Layout = "${onexception:inner=${callsite:includeNamespace=false:fileName=true:includeSourcePath:false} }${message} ${onexception:inner=${newline}    ${exception}}"
            };
            nConfig.AddTarget(consoleTarget);
            nConfig.AddRule(consoleMinLevel, NLog.LogLevel.Fatal, consoleTarget);


            if (logLevel <= LogLevel.Debug)
            {
                //Console Target for Debug
                var consoleTargetDebug = new ColoredConsoleTarget("logconsoledebug")
                {
                    Layout = @"${level:uppercase=true} ${message} ${onexception:inner=${newline}    ${exception}}"
                };
                nConfig.AddTarget(consoleTargetDebug);
                nConfig.AddRule(NLog.LogLevel.FromOrdinal((int)logLevel), NLog.LogLevel.Debug, consoleTargetDebug);
            }


            //Default Null Rule for Microsoft.*
            nConfig.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Info, new NullTarget(), "Microsoft.*", true);

            //File target for Debug
            var fileTarget = new FileTarget("logfile")
            {
                FileName = $"{Application.StartupPath}/${{processname}}_debug_${{shortdate}}.log",
                Layout = @"${onexception:inner=${callsite:includeNamespace=false:fileName=true:includeSourcePath:false} }${message} ${onexception:inner=${newline}    ${exception}}"
            };
            nConfig.AddTarget(fileTarget);
            nConfig.AddRuleForAllLevels(fileTarget);



            /*
            //Add a rule for web processing if that's enabled
            if (addWeb)
            {
                var allFileTarget = new FileTarget("allFile")
                {
                    FileName = "H:/source/repos/VideoIntelligence/logs/${processname}_all_${shortdate}.log",
                    Layout = "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}"
                };
                nConfig.AddTarget(allFileTarget);
                nConfig.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, allFileTarget, "*");
            }
            


            //File target for Errors and up
            var fileTargetError = new FileTarget("errorlogfile")
            {
                FileName = "H:/source/repos/VideoIntelligence/logs/${processname}_errors_${shortdate}.log",
                Layout = @"${longdate}|${level:uppercase=true}|${callsite:fileName=true}|${message}|${exception:format=tostring}|${logger}|${all-event-properties}"
            };
            nConfig.AddTarget(fileTargetError);
            nConfig.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, fileTargetError);


            //Add a rule for web processing if that's enabled
            if (addWeb)
            {
                var webFileTarget = new FileTarget("ownFile-web")
                {
                    FileName = "H:/source/repos/VideoIntelligence/logs/${processname}_web_${shortdate}.log",
                    Layout = "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}"
                };
                nConfig.AddTarget(webFileTarget);
                nConfig.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, webFileTarget, "*");
            }
            */

            NLog.LogManager.Configuration = nConfig;
            //NLog.LogManager.ReconfigExistingLoggers();
        }
    }
}
