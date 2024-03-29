# Cloud-native Weather Service with .NET Core

This example implements a simple weather REST service using .NET Core.

![Weather Service Architecture](architecture.png)

## Build and run locally

```bash
$ tilt up
$ skaffold dev --no-prune=false --cache-artifacts=false
```

## Exercise the application

```bash
$ curl -X GET http://localhost:18080/api/weather\?city\=Rosenheim
{"city":"Rosenheim","weather":"Clear"}

$ curl -X GET http://localhost:18080/
$ open http://localhost:18080/
```

## Lab Instructions

The instructions for the Cloud-native Experience Lab workshop can be found in [docs/README.md](docs/README.md).

## Maintainer

M.-Leander Reimer (@lreimer), <mario-leander.reimer@qaware.de>

## License

This software is provided under the Apache v2.0 open source license, read the `LICENSE`
file for details.
