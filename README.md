# OpenTelemetry.Heartbeat
A simple worker that Converts files into probes that is exposed as a metric.

**example heartbeat.json**
```json
{
  "name": "My Service",
  "namespace": "My Namespace",
  "interval": 1000, // in milliseconds
  "monitor": {
    "type": "http",
    "timeOut": 1000, // in milliseconds
    "url": "https://localhost",
    "responseCode": 200
  }
}
```
the metric will follow the [Otel standard](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/resource/semantic_conventions/README.md) and contain the following labels
- service.name (name from the json)
- service.namespace (namespace from the json)

when configuring make sure that the `interval` defined in appsettings.heartbeatsettings is greater than the `timeout` specified in the heartbeat.json files, otherwise the worker will not be able to keep up with the probes.

## How to use
Run the Service and configure the appsettings.json files to point to the folder where the heartbeat.json files are located.

when deploying a service, call the endpoint POST /monitors to add, modify or remove a monitor. see swagger for more details.

## Why
[OpenTelemetry HttpStatucCheck reciever](https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/receiver/httpcheckreceiver/documentation.md) required that all urls be known at the deployment time of the collector.   
This worker is intended to be used in a setup where you do not know the urls beforehand and you do not have any service discovery mechanism in place.


