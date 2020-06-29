namespace ClickView.CloudWatch.Metrics.RabbitMq
{
    using System;
    using Amazon.CloudWatch;
    using EasyNetQ.Management.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Options;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions<WorkerOptions>()
                        .Configure(o =>
                        {
                            var config = hostContext.Configuration.GetSection("Worker");
                            o.Interval = TimeSpan.FromSeconds(config.GetValue<int>("Interval"));
                        })
                        .ValidateDataAnnotations();

                    services.AddOptions<AmazonCloudWatchOptions>()
                        .Configure(o =>
                        {
                            var config = hostContext.Configuration.GetSection("Aws");
                            o.AccessKeyId = config["AccessKeyId"];
                            o.SecretAccessKey = config["SecretAccessKey"];
                        })
                        .ValidateDataAnnotations();

                    services.AddOptions<ManagementClientOptions>()
                        .Configure(o =>
                        {
                            var config = hostContext.Configuration.GetSection("RabbitMq");
                            o.Host = config["Host"];
                            o.Username = config["Username"];
                            o.Password = config["Password"];
                        })
                        .ValidateDataAnnotations();

                    services.AddHostedService<Worker>();

                    services.AddSingleton<IAmazonCloudWatch>(p =>
                    {
                        var options = p.GetRequiredService<IOptionsMonitor<AmazonCloudWatchOptions>>().CurrentValue;

                        return string.IsNullOrWhiteSpace(options.AccessKeyId) ||
                               string.IsNullOrWhiteSpace(options.SecretAccessKey)
                            ? new AmazonCloudWatchClient()
                            : new AmazonCloudWatchClient(options.AccessKeyId, options.SecretAccessKey);
                    });

                    services.AddSingleton<IManagementClient>(p =>
                    {
                        var options = p.GetRequiredService<IOptionsMonitor<ManagementClientOptions>>().CurrentValue;
                        return new ManagementClient(options.Host, options.Username, options.Password);
                    });
                });
    }
}