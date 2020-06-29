# ClickView.CloudWatch.Metrics.RabbitMq

## Overview

Publish RabbitMQ metrics to AWS CloudWatch

## Supported metrics

| Metric Name              | Description                                  |
| ------------------------ | -------------------------------------------- |
| `MessagesReady`          | Number of messages waiting to be processed   |
| `MessagesUnacknowledged` | Number of messages currently being processed |
| `MessagesTotal`          | Total number of messages                     |
| `ConsumersActive`        | Number of active consumers                   |
| `ConsumersTotal`         | Total number of consumers                    |

## Usage

``` 
docker run -d \
  --name=cloudwatch-metrics-rabbitmq \
  -e WORKER__INTERVAL=5 \
  -e AWS__ACCESSKEYID=myAccessKeyId \
  -e AWS__SECRETACCESSKEY=mySecretAccessKey \
  -e RABBITMQ__HOST=rabbitMqHost \
  -e RABBITMQ__USERNAME=admin \
  -e RABBITMQ__PASSWORD=password \
  cloudwatch-metrics-rabbitmq
```

## Environment variables

| Parameter              | Description                            |
| ---------------------- | -------------------------------------- |
| `WORKER__INTERVAL`     | Interval in seconds to publish metrics |
| `AWS__ACCESSKEYID`     | The AWS Access Key ID (optional)       |
| `AWS__SECRETACCESSKEY` | The AWS Secret Access Key (optional)   |
| `RABBITMQ__HOST`       | The RabbitMQ host                      |
| `RABBITMQ__USERNAME`   | The RabbitMQ username                  |
| `RABBITMQ__PASSWORD`   | The RabbitMQ password                  |
