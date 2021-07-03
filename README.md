# AcmeChallengeResponder

This is a simple and lightweight service that supports remote validation when requesting HTTPS certificates from
Let's Encrypt or another ACME-compatible server.

It works with the [HTTP-01 Challenge Type](https://letsencrypt.org/docs/challenge-types/#http-01-challenge) and
requires an ACME client which can interact with this service. One such client is [win-acme](https://www.win-acme.com/)
using the [REST plugin](https://www.win-acme.com/reference/plugins/validation/http/rest).



## Why remote validation?

Probably the simplest way to handle HTTP-01 Challenge is to have the ACME client publish a specific file at
`http://<YOUR_DOMAIN>/.well-known/acme-challenge/<TOKEN>` or making the ACME client itself listen to HTTP
requests from the ACME server in order to supply the expected responses. The win-acme client is capable of doing
that and more via one of its [HTTP validation plugins](https://www.win-acme.com/reference/plugins/validation/http/),
but you need to either run the ACME client on your server or have a way to publish a file to a specific location
that the webserver software knows about and can read and serve back to the ACME server.

That works fine for many uses cases, but:
- you might need to run the ACME client from a different machine, like a CI/CD server or a secured workstation;
- you don't have direct access to the server itself and can't run scheduled jobs there;
- you don't have an easy way to publish files to the server, like a file share or a FTP service;
- you have more than one server handling web trafic;
- you might have multiple domains to validate and want a single certificate for them all.

There might be good solutions to many of the points made above, but using a remote validation solution like
AcmeChallengeResponder might be a better and easier approach.



## How does this service works?

AcmeChallengeResponder is a REST endpoint you run on your server. It receives validation challenges from
the ACME client and serves them back to the ACME server during the validation process.

The ACME client publishes challenge responses to AcmeChallengeResponder by issuing HTTP PUT or DELETE
requests. Those requests need to be authenticated by a security token, see [Security](#security) bellow.



## Features

- Simple and lightweight;
- Holds challenge responses in memory, no external storage backend required;
- Can be deployed to IIS as an application without interfering with normal traffic;
- Can be run stand-alone in case you don't use IIS or don't want to deploy to IIS.



## Security

When deploying this service you need to modify `appsettings.json` and configure a security token. This acts
as a password in order to be able to publish/unpublish challenge responses to the server. The security token
should be a secure random value only known to those involved in managing HTTPS certificates for the server(s).

Bellow is a little PowerShell script which generates a 512-bit cryptographic random value and outputs it
using base64 encode:

```powershell
$bytes = [byte[]]::new(64);
[System.Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes);
[Convert]::ToBase64String($bytes);
```



## Installation and configuration

First of all you need to modify `appsettings.json` file and replace `FIX_ME` with some secure value as
explained above.

This is an ASP.NET Core 5.0 application, so you need .NET 5 installed on the server in order to use it. You might
be able to build the application from source code using another .NET version, but we only distribute packages for
.NET 5.

This application can be hosted in IIS alongside another applications or you can run it stand-alone.


### Hosting in IIS

This is the preferred way of using AcmeChallengeResponder. For a complete explanation and details of how to deploy
an ASP.NET Core application to IIS refer to the
[official documentation](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-5.0).

In order to work properly, this application MUST be configured to receive requests at
`http[s]://<your domain>/.well-known/` or `http[s]://<your domain>/.well-known/acme-challenge/`, for
example http://www.example.org/.well-known/. If you don't already have any content or application being
served under the `/.well-known`, use it as the application path, else use `/.well-known/acme-challenge`.

Using IIS Manager (or `appcmd.exe` or `PowerShell` or any other method), add a new Application to the Website
you with to publish AcmeChallengeResponder to and name it `.well-known` as explained above. Select
the Application Pool you wish to use and inform the physical path to the extracted files, like
`C:\inetpub\wwwroot\AcmeChallengeResponder`. An IIS Application Pool can only run one ASP.NET Core application,
so it it recommended to create a new Application Pool.

Also make sure `Anonymous Authentication` is enabled for AcmeChallengeResponder.



## Requesting a certificate

TODO: show a complete example of requesting a certificate from a remote machine using AcmeChallengeResponder.



## Changelog

- v1.1 First public release



## TODO

- Being able to support an external storage for challenge tokens, for cases when there are multiple active
servers handling requests for the same domain.
