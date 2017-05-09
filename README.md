# ZeroKit-Dotnet-admin-api-client
[![Build Status](https://travis-ci.org/tresorit/ZeroKit-Dotnet-admin-api-client.svg?branch=master)](https://travis-ci.org/tresorit/ZeroKit-Dotnet-admin-api-client.svg?branch=master)

Small client lib to call ZeroKit's signed administrative API from .net.
This lib provides a special HTTP client which automatically signs the administrative requests for your ZeroKit tenant's admin API.

More information about ZeroKit encryption platform: [https://tresorit.com/zerokit](https://tresorit.com/zerokit)

ZeroKit management portal: [https://manage.tresorit.io](https://manage.tresorit.io)

## Example
```csharp
using System;
using Tresorit.ZeroKit.AdminApiClient;

namespace Tresorit.ZeroKit.AdminApiClient.Example
{
	class Example
	{
		// Response class for auto-json serialization
		public class InitUserRegistrationResponse
		{
			public string UserId { get; set; }
			public string RegSessionId { get; set; }
			public string RegSessionVerifier { get; set; }
		}

		static void Main(string[] args)
		{
			try
			{
				// Provider your zeroKit tenant's settings
				var client = new AdminApiClient("ServiceUrl", "AdminKey");

				// Assemble call and do the request (you can also use async api)
				var response = client.CreatePostRequest("/api/v4/admin/user/init-user-registration").Query().AsJson<InitUserRegistrationResponse>();

				// Use returned data
				var userId = response.UserId;
			}
			catch (AdminApiException e)
			{
				// Handle API errors
				Console.WriteLine($"An error occured. Api error code: {e.ErrorCode}, error message: {e.Message}");
			}
		}
	}
}

```

## Notes
The library supports both asynchronous and synchronous APIs. We highly recommend the usage of async io for new projects.
You can use this lib under both MS .net and Mono framework.