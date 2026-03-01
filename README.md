# ShipDataViewer
A small C#/WPF desktop application that listens and displays ship data from https://aisstream.io/

## Architecture
The `ShipDataViewer.Core` project contains the model and the interface of the service. These are used in the:  
- `AisStream` project which implements the service interface and publishes ship data as core models. `AisStream` project also uses the generated `JSON` models for parsing from `Org.OpenAPITools`.
- `ShipDataViewer.Shell` project to listen to the service (through the core service interface) and load ship data as core models. The only place where `ShipDataViewer.Shell` references `AisStreamService` (the implementation of the core service) from the `AisStream` project is in `Bootstrapper` which contains IoC container type registrations. This way, `ShipDataViewer.Shell` stays dependent only on `ShipDataViewer.Core` models and service, not on any specific implementation.

## Extra notes
In a realistic scenario, the `Org.OpenAPITools` and the `AisStream` project would be in another repository and published as a nuget which would then be installed in `ShipDataViewer.Shell`.

This wasn't done here because it would be too cumbersome.

## How to update to a new version (or have another service implementation)
When https://aisstream.io/ would change their API, we would regenerate the `JSON` models based on the updated `yaml` type definitions, adjust the code accordingly, but publish the same core models to ensure that nothing changes for the users of the service.

For another service implementation we would simply have to create a new project that has a dependency on the `ShipDataViewer.Core` project (as a nuget) and then publish another nuget for the new implementation which would then be installed in `ShipDataViewer.Shell` and the `Bootstrapper` would have to be updated.

Example for a service implementation called `FakeService`:
```cs
// Boostrapper.cs
builder.RegisterType<FakeService>().AsImplementedInterfaces();
```
After rebuilding, the `ShipDataViewer.Shell` would use the new service implementation. No other code changes required because it already uses the core model.

## Future idea
Implement a plugin architecture: just copy-paste a DLL with any implementation and select from the app interface which implementation you want to use.

## Examples and generated code
The code in `AisStreamService` is based on this example:  
https://github.com/aisstream/example/blob/main/csharp/Program.cs

The `Org.OpenAPITools` project is generated from this OpenAPI type definition:  
https://github.com/aisstream/ais-message-models/blob/master/type-definition.yaml

```
dotnet tool install --global OpenAPI.Generator.CLI

openapi-generator-cli generate
  -i type-definition.yaml \
  -g csharp \
  -o Generated/
```

## Code formatting
To keep `xaml` code better organized, I used this extension for Visual Studio:  
https://marketplace.visualstudio.com/items?itemName=TeamXavalon.XAMLStyler2022


## Libraries
The app uses the following libraries: 
- `Fody.PropertyChanged` in order to automatically generate the `PropertyChanged` boilerplate.
- `Stylet` for MVVM style app boilerplate.

The things I like most about `Stylet` are:  
1. that it's view model first (you create the view model using an IoC container) and then `Stylet` handles creating the view and assigning the `DataContext` to the view model. It's based on the expectation that the View always has the same name as the ViewModel (without the "Model" part, example: `ShellViewModel` --> `ShellView`, `SomeNameViewModel` --> `SomeNameView`)
2. that it allows you to skip defining/implementing `ICommand` and instead you can bind directly to a method from view model. Example:  

```cs
public async Task StartListeningAsync()
{
    // code
}
```

and in `xaml`:  
```xml
<!-- code -->
xmlns:s="https://github.com/canton7/Stylet"
<!-- more code -->

<Button Command="{s:Action StartListeningAsync}" Content="Start listening" />
```
