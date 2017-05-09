using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Tresorit.ZeroKit.AdminApiClient.Test.ApiObjects;

namespace Tresorit.ZeroKit.AdminApiClient.Test
{
	[TestFixture]
	public class Tests
	{
		private string ServiceUrl;

		private string AdminKey;

		public Tests()
		{
			this.ServiceUrl = Environment.GetEnvironmentVariable("ZKIT_SERVICE_URL");
			this.AdminKey = Environment.GetEnvironmentVariable("ZKIT_ADMIN_KEY");

			if (this.ServiceUrl == null || this.AdminKey == null)
				throw new Exception("Failed to load service url and / or admin key from the environment!");
		}

		[Test]
		public void CanNotBeCreatedWithNullUrl()
		{
			Assert.Throws<ArgumentNullException>(() => new AdminApiClient(null, AdminKey));
		}

		[Test]
		public void CanNotBeCreatedWithBadUrl()
		{
			Assert.Catch<ArgumentException>(() => new AdminApiClient("badurl://bad.bad", AdminKey));
		}

		[Test]
		public void CanNotBeCreatedWithNullAdminKey()
		{
			Assert.Throws<ArgumentNullException>(() => new AdminApiClient(ServiceUrl, null));
		}

		[Test]
		public void CanNotBeCreatedWithShortAdminKey()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new AdminApiClient(ServiceUrl, AdminKey.Substring(2)));
		}

		[Test]
		public void CanNotBeCreatedWithNonHexAdminKey()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new AdminApiClient(ServiceUrl, "no" + AdminKey.Substring(2)));
		}

		[Test]
		public void CanNotBeCreatedWithBadTenantId()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new AdminApiClient(ServiceUrl, AdminKey, "00testtest"));
		}

		[Test]
		public void CanNotBeCreatedWithShortTenantId()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new AdminApiClient(ServiceUrl, AdminKey, "nope"));
		}

		[Test]
		public void CanBeCreated()
		{
			var client = new AdminApiClient(ServiceUrl, AdminKey);

			Assert.IsNotNull(client);
		}

		[Test]
		public async Task CanCallJsonApiWithoutPayload()
		{
			var client = new AdminApiClient(ServiceUrl, AdminKey);

			var response = await client.
				CreatePostRequest("/api/v4/admin/user/init-user-registration").
				QueryAsync().
				AsJsonAsync<InitUserRegistrationResponse>();

			Assert.NotNull(response);
			Assert.IsInstanceOf<InitUserRegistrationResponse>(response);
			Assert.IsNotNull(response.UserId);
			Assert.IsNotNull(response.RegSessionId);
			Assert.IsNotNull(response.RegSessionVerifier);
		}

		[Test]
		public void CanCallJsonApiSynchronously()
		{
			var client = new AdminApiClient(ServiceUrl, AdminKey);

			var response = client.
				CreatePostRequest("/api/v4/admin/user/init-user-registration").
				Query().
				AsJson<InitUserRegistrationResponse>();

			Assert.NotNull(response);
			Assert.IsInstanceOf<InitUserRegistrationResponse>(response);
			Assert.IsNotNull(response.UserId);
			Assert.IsNotNull(response.RegSessionId);
			Assert.IsNotNull(response.RegSessionVerifier);
		}

		[Test]
		public async Task CanCallJsonApiWithPayload()
		{
			var client = new AdminApiClient(ServiceUrl, AdminKey);

			var response = await client.
				CreatePutRequest("/api/v4/admin/tenant/upload-custom-content?fileName=css/login.css").
				SetHeader("Content-Type", "text/css").
				QueryAsync("body { background-color: red; }").
				AsJsonAsync<UploadCustomContentResponse>();

			Assert.NotNull(response);
			Assert.IsInstanceOf<UploadCustomContentResponse>(response);
			Assert.IsNotNull(response.Name);
			Assert.IsNotNull(response.Path);
			Assert.IsNotNull(response.Url);
			Assert.IsNotNull(response.Size);
			Assert.IsNotNull(response.ContentType);
			Assert.IsNotNull(response.Etag);
			Assert.AreEqual("text/css", response.ContentType);
		}

		[Test]
		public async Task CanCallJsonApiWithPayloadApiError()
		{
			var client = new AdminApiClient(ServiceUrl, AdminKey);

			var response = await client.
				CreatePostRequest("/api/v4/admin/user/init-user-registration").
				QueryAsync().
				AsJsonAsync<InitUserRegistrationResponse>();

			try
			{
				await client.
					CreatePostRequest("/api/v4/admin/user/set-user-state").
					QueryJsonAsync(new SetUserStateRequest() { UserId = response.UserId, Enabled = false }).
					AsStringAsync();

				Assert.Fail("API exception should be thrown!");
			}
			catch (AdminApiException aex)
			{
				Assert.AreEqual("UserNotExists", aex.ErrorCode);
			}
		}
	}
}
