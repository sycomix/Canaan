using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

using Canaan;

[assembly: WebJobsStartup(typeof(NewsAlpha.Functions.Startup))]
namespace NewsAlpha
{
    public static class FourChanAzureFunctions
    {
        [FunctionName("Update"), ]
        public static async Task Update([TimerTrigger("0 */13 * * * *")] TimerInfo myTimer, TraceWriter log)
        {
            Serilog.Core.Logger logger = new LoggerConfiguration()
                     .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                     .MinimumLevel.Debug()
                     .Enrich.FromLogContext()
                     .WriteTo.TraceWriter(log)
                     .CreateLogger();
            Api.SetLogger(new SerilogLogger(logger));
            FourChanAzurePipeline pipeline = new FourChanAzurePipeline();
            await pipeline.Update("pol");            
        }
    }
}
