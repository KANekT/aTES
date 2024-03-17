// Copyright 2020 Confluent Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Refer to LICENSE for more information.

using System.Diagnostics;
using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Core.Kafka;

/// <summary>
///     Middleware that times how long a web request takes to be handled,
///     and logs this to Kafka.
/// </summary>
public class RequestTimerMiddleware
{
    private readonly IKafkaDependentProducer<string, Proto.V1.RequestTimeProto> _producer;
    private readonly IKafkaDependentProducer<string, Proto.V2.RequestTimeProto> _producerV2;
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public RequestTimerMiddleware(RequestDelegate next, 
        IKafkaDependentProducer<string, Proto.V1.RequestTimeProto> producer, 
        IKafkaDependentProducer<string, Proto.V2.RequestTimeProto> producerV2, 
        ILogger<RequestTimerMiddleware> logger)
    {
        _next = next;
        _producer = producer;
        _producerV2 = producerV2;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var s = new Stopwatch();
        try
        {
            s.Start();
            await _next(context);
        }
        finally
        {
            s.Stop();

            // Write request timing infor to Kafka (non-blocking), handling any errors out-of-band.
            var pathValue = context.Request.Path.Value;
            if (!string.IsNullOrEmpty(pathValue))
            {
                /*
                _producer.Produce(
                    Constants.KafkaTopic.RequestTime,
                    new Message<string, Proto.V1.RequestTimeProto>
                    {
                        Key = pathValue,
                        Value = new Proto.V1.RequestTimeProto
                        {
                            EventId = Ulid.NewUlid().ToString(),
                            EventName = Constants.KafkaTopic.RequestTime + "_1",
                            EventTime = DateTime.UtcNow.ToString("u"),
                            EventVersion = "1",
                            Time = s.ElapsedMilliseconds
                        }
                    },
                    _deliveryReportHandler
                );
                */
                
                _producerV2.Produce(
                    Constants.KafkaTopic.RequestTime,
                    new Message<string, Proto.V2.RequestTimeProto>
                    {
                        Key = pathValue, 
                        Value = new Proto.V2.RequestTimeProto
                        {
                            EventId = Ulid.NewUlid().ToString(),
                            EventName = Constants.KafkaTopic.RequestTime + "_2",
                            EventTime = DateTime.UtcNow.ToString("u"),
                            EventVersion = "2",
                            Time = s.ElapsedMilliseconds,
                            Path = pathValue
                        }
                    },
                    _deliveryReportHandler
                );
            }
        }
    }
    
    private void _deliveryReportHandler(DeliveryReport<string, Proto.V2.RequestTimeProto> deliveryReport)
    {
        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            _logger.Log(LogLevel.Warning, $"Failed to log request time for path: {deliveryReport.Message.Key}");
        }
    }
}