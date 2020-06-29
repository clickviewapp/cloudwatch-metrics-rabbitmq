namespace ClickView.CloudWatch.Metrics.RabbitMq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.CloudWatch;
    using Amazon.CloudWatch.Model;
    using EasyNetQ.Management.Client;
    using EasyNetQ.Management.Client.Model;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Options;

    public class Worker : IHostedService, IDisposable
    {
        private readonly IAmazonCloudWatch _awsCloudWatchClient;
        private readonly IManagementClient _rabbitMqClient;
        private readonly ILogger<Worker> _logger;

        private readonly TimeSpan _interval;

        private Timer _timer;

        public Worker(
            IAmazonCloudWatch awsCloudWatchClient,
            IManagementClient rabbitMqClient,
            ILogger<Worker> logger,
            IOptionsMonitor<WorkerOptions> options
        )
        {
            _awsCloudWatchClient = awsCloudWatchClient;
            _rabbitMqClient = rabbitMqClient;
            _logger = logger;

            _interval = options.CurrentValue.Interval;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting worker...");

            _timer = new Timer(async _ => await LoopAsync(cancellationToken), null, TimeSpan.Zero, _interval);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping worker...");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async Task LoopAsync(CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation("Fetching metrics...");

                var queues = await _rabbitMqClient.GetQueuesAsync(token);

                var metrics = queues.SelectMany(CreateMetrics).ToList();
                
                _logger.LogInformation("Publishing metrics...");

                await _awsCloudWatchClient.PutMetricDataAsync(new PutMetricDataRequest
                {
                    MetricData = metrics,
                    Namespace = "RabbitMQ"
                }, token);
                
                _logger.LogInformation("Finished publishing metrics");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured");
            }
        }

        private static IEnumerable<MetricDatum> CreateMetrics(Queue queue)
        {
            return new[]
            {
                CreateMetric(queue.Name, "MessagesReady", queue.MessagesReady),
                CreateMetric(queue.Name, "MessagesUnacknowledged", queue.MessagesUnacknowledged),
                CreateMetric(queue.Name, "MessagesTotal", queue.Messages),
                CreateMetric(queue.Name, "ConsumersActive", queue.ActiveConsumers),
                CreateMetric(queue.Name, "ConsumersTotal", queue.Consumers)
            };
        }
        
        private static MetricDatum CreateMetric(string queueName, string name, double value)
        {
            return new MetricDatum
            {
                Dimensions = new List<Dimension>
                {
                    new Dimension
                    {
                        Name = "QueueName",
                        Value = queueName
                    }
                },
                MetricName = name,
                Unit = StandardUnit.Count,
                Value = value
            };
        }
    }
}