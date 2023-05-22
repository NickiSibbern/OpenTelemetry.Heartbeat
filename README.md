# OpenTelemetry.Heartbeat
A simple worker that Converts files into probes that is exposed as a metric. The file is expected to be a json file with the following format:


**HttpMonitor**
```json
{
  "name": "My Service",
  "namespace": "My Namespace",
  "interval": 10,
  "type": "http",
  "http": {
    "timeOut": 100,
    "url": "https://localhost",
    "responseCode": 200
  }
}
```
the metric will follow the [Otel standard](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/resource/semantic_conventions/README.md) and contain the following labels
- service.name (name from the json)
- service.namespace (namespace from the json)

## Why
[OpenTelemetry HttpStatucCheck reciever](https://github.com/open-telemetry/opentelemetry-collector-contrib/blob/main/receiver/httpcheckreceiver/documentation.md) required that all urls be known at the deployment time of the collector.   
This worker is intended to be used where you do not know the urls beforehand and you do not have any service discovery mechanism in place.