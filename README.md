# Pepegov.Identity.OpenIddict

This project is a flexible implementation of microservice architecture with OpenIddict authentication server

Implemented next stack:
- Definition. Simple "Vertical Slice architecture"
- Reposytory
- UnitOfWork
- OpenIddict
- AuthoMapper
- MassTransit
- Swagger with identity
- IOption 

More information on OpenIddict can be found [here](https://documentation.openiddict.com/)

More OpenIddictTemplate [here](https://github.com/openiddict/openiddict-samples)

The template idea was taken from [Calabonga](https://github.com/Calabonga/Microservice-Template)

<br>

---

<br>

## How to install template

To install a template for your device via dotnet:

1. Go to the directory with the desired template
    ```bash
    cd OpenIddictService/Service
    ```
2. Perform the installation
    ```bash
    dotnet new --install .
    ```

To install a template for your device via dotnet:

1. Go to File => New Solution... => More Templates => Install Template...
2. Select folder with .sln file (OpenIddict/Service/OpenIddictServiceTemplate)

<br>

---

<br>

## Configuration

### APPSETTING.JSON & APPSETTING.JSON.DEVELOPER 

Cors permissions. This example is for appsetting.Development.json For regular appsetting.json settings needed by cors

```json
"Cors": {
    "Origins": "*"
}  
```

Admin user for connection testing

```json
"AdminUser": {
    "Email": "admin@microservice.net",
    "Password" : "qwe123!@#",
    "UserName": "admin",
    "FirstName": "Identity",
    "LastName": "Administrator",
    "PhoneNumber": "+79000000000",
    "EmailConfirmed": true,
    "PhoneNumberConfirmed": true
}
```
Connection string for your PostgreSQL database

```json 
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=Microservice.Identity"
}
```
Configuring logging via serilog. Based on the needs, configure sinks. Note that appsetting.Development.json should have a "MinimumLevel" less than appsetting.json

```json
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File"
    ],
    "MinimumLevel": {
      "Default": "Debug",
    
    ...
```

### IDENTITYSETTING.JSON

The address of the identity server. Must match the address of the application.

```json
"IdentityServerUrl": {
    "Authority": "https://localhost:10001"
}
```

List of scopes that the server supports

```json
"Scopes": [
    {
        "Name": "Microservice.Profile",
        "Description": "Microservice Profile API"
    }
],
```

List of clients that the server support

```json
"ClientsIdentity": [
    {
      "Name" : "MicroserviceCredentials",
      "Id" : "MicroserviceCredentials-4523f-21321",
    
    ...
    }
]
```

The current server client that will be substituted in swagger. You need to specify all possible connections for it. It supports all supported scopes at once

```json
"CurrentIdentityClient": {
    "Name" : "Microservice.Identity",
    "Id" : "Microservice.Identity-4523f-21321",
    
    ...
  }, 
```

Permissions for asp.net policy. Added to the "Permissions" table in the database, also added to the admin user profile

```json
  "Permissions": [
    {
      "Name": "Permission:Get",
      "Description": "Get permission"
    },
    ...
```

#### ClientsIdentity syntaxis:

- "Name" : "Display name",

- "Id" : "Id core for client",

- "Secret": "Secret key for client",

- "GrandTypes": [
    "Possible connection options"
    ],

- "Scopes": [
"Your services"
]

- "RedirectUris": [
    "Redirect uris for authorization flow"
    ]

<br>

---

<br>

## Launch

In order for the data to be transmitted via https, you need to get certificates. For windows and some linux
distributions, use [dev-cents](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs):

```bash
dotnet dev-certs https
```

Launch "https" launchsetting

### DEV-CENTS NOT SUPPORT

If your distribution does not support this command, then use the script in the "**dev-cent**" folder:

```bash
cd dev-cert
./create-dev-cert.sh
```

You will be required to enter a password. Enter "**password**" 2 times

If you did everything right, then
launch "**httpsWithLocalCert**" launchsetting

<br>

---

<br>

## Testing

To test authentication, you can use swagger in Developer mode or Thunder Clinet for vscode

### Swagger 

Swagger takes standard authentication data from "CurrentIdentityClient" fron identity setting.json

On the top right, select "Authorize" and log in via the desired flow

### Thunder Clinet

Install [Thunder Clinet](https://marketplace.visualstudio.com/items?itemName=rangav.vscode-thunder-client) from marketplace or vscode

Click to Thunder Clinet icon in vscode or press Ctrl+Shift+R. 

Click "Collection" and open the menu => "Import". Open file "OpenIddictService/TestCollections/thunder-collection_OpenIddict"

<br>

---

<br>

## Project Structure

<br>

### DAL (Data Access Layer)

The DAL stores classes or tools for working with the database.
Also stores data for the operation of the application

**Domain**
- AppData. Application description (service name, description, version, etc.) 
- AuthData. Authentication data

**Model**

Application models. Mostly 
information for the database

**Database**
- ApplicationDbContext. Context for entity framework
- DatabaseInitializer. Сreates a database and initializes the initial data into it

<br>

### Identity

The main code of the application

**Folders**

- Application/Services. Services and their repositories
- Definitions. "Slice" list for program.cs
- EndPoint. The endpoint of the request. Stored Here ASP.NET MinimalAPI or consumers MassTransit.
- Pages. Razor page for 
auntifications code flow

<br>

---

<br>

## Working with project

<br>

### Definitions

A set of folders that you can delete, copy, add new ones. Standard integrations for the application are implemented, such as working with openaddict, swagger, unitofwork, etc.

Allows you to conveniently add services. To create your own definition:

- Create a folder in "Definitions" folder
- Create a .cs file in a new folder
- Inherit the "Definition" class and override the "ConfigureService" method
- Inside the method, add the necessary services

It should look something like this:

```C#
public class MyDefinition : Definition
{
    public override void ConfigureServiceAsync(IServiceCollection services, WebApplicationBuilder builder)
    {
        Object myService = new();
        services.AddSingleton(myService);
    }

    public override void ConfigureApplicationAsync(WebApplication app)
    {
        app.UseMyService();
    }
}
```

### EndPoints

They are also added as definitions. The endpoint folder should store the entities that are used by this endpoint.

The "EndPoint" file must have a minimalapi implementations.

It is allowed to have such entities as:

- ViewModel
- Helper
- Consumer (MassTransit)
- Request (MediatoR)

The best solution would be to use MediatoR as a reference to the Api

It look like:

- MinimalApi (ASP.NET) => Request (MediatoR)

- Consumer (MassTransit) => Request (MediatoR)
