# ShipDataViewer
A small C#/WPF desktop application that listens and displays ship data from https://aisstream.io/

## Small overview
The app uses the following libraries: `Fody.PropertyChanged` in order to automatically generate the `PropertyChanged` boilerplate.

The `Org.OpenAPITools` project is generated from this OpenAPI type definition:  
https://github.com/aisstream/ais-message-models/blob/master/type-definition.yaml

## Architecture
The `ShipDataViewer.Core` project contains the model and the interface of the service. These are used in the:  
- `AisStream` project which implements the service interface and publishes ship data as core models. `AisStream` project also uses the generated `JSON` models for parsing from `Org.OpenAPITools`.
- `ShipDataViewer.Shell` project to listen to the service (through the core service interface) and load ship data as core models. The only place where `ShipDataViewer.Shell` references `AisStreamService` (the implementation of the core service) from the `AisStream` project is in `Bootstrapper` which contains IoC container type registrations. This way, `ShipDataViewer.Shell` stays dependent only on `ShipDataViewer.Core` models and service, not on any specific implementation.

