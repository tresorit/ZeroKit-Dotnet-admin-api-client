using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tresorit.ZeroKit.AdminApiClient
{
    /// <summary>
    /// Administrative API client
    /// </summary>
    /// <remarks>
    /// This is a small HTTP client that makes it easy to call ZeroKit' signed admin api
    /// </remarks>
    public class AdminApiClient
    {
		/// <summary>
		/// Regex to recognize and parse production tenant service URLs
		/// </summary>
		protected static readonly Regex ProductionTenantUrlRegex = new Regex(@"\Ahttps?\:\/\/(?<tenantid>[a-z][a-z0-9]{7,9})\.api\.tresorit\.io\/?\z", RegexOptions.Compiled);

		/// <summary>
		/// Regex to recognize and parse hosted tenant service URLs
		/// </summary>
		protected static readonly Regex HostedTenantUrlRegex = new Regex(@"\Ahttps?\:\/\/[^\/?#]*\/tenant-(?<tenantid>[a-z][a-z0-9]{7,9})\/?\z", RegexOptions.Compiled);

		/// <summary>
		/// Regex to recognize and validate tenant admin keys
		/// </summary>
		protected static readonly Regex AdminKeyRegex = new Regex(@"\A[a-fA-F0-9]{64}\z", RegexOptions.Compiled);

		/// <summary>
		/// Regex to recognize and validate tenant IDs
		/// </summary>
		protected static readonly Regex TenantIdRegex = new Regex(@"\A[a-z][a-z0-9]{7,9}\z", RegexOptions.Compiled);
		
		/// <summary>
		/// Gets the tenant ID of the bound tenant
		/// </summary>
		public string TenantId { get; }

		/// <summary>
		/// Gets the service URL of the bound tenant
		/// </summary>
	    public Uri ServiceUrl { get; }

		/// <summary>
		/// Gets the admin usre id of the bound tenant
		/// </summary>
	    public string AdminUserId { get; }

		/// <summary>
		/// Gets the admin key of the tenant
		/// </summary>
	    protected internal byte[] AdminKey;

		/// <summary>
		/// Underlying HTTP client
		/// </summary>
	    protected HttpClient HttpClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="AdminApiClient"/> class
		/// </summary>
		/// <remarks>
		/// You can find the necessary paramaters at the main page of your tenant on the zeroKit
		/// management portal (https://manage.tresorit.io).
		/// </remarks>
		/// <example>
		/// // Typical usage
		/// var client = new AdminApiClient("https://yourtenant.api.tresorit.io", "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")
		/// </example>
		/// <param name="serviceUrl">Service URL of the tenant, copied from the management portal</param>
		/// <param name="adminKey">One of the admin keys of the tenant, copied from the management portal (it's a 64 char long hexadecimal string)</param>
		/// <param name="tenantId">[OPTIONAL] If your tenant is hosted on a special URL and the tenant id cannot be inferred from your service URL, then you should also supply the tenant id.</param>
		public AdminApiClient(string serviceUrl, string adminKey, string tenantId = null)
	    {
			// Check input
			if (serviceUrl == null)
				throw new ArgumentNullException(nameof(serviceUrl));
			if (adminKey == null)
				throw new ArgumentNullException(nameof(adminKey));
			if (!AdminKeyRegex.IsMatch(adminKey))
				throw new ArgumentOutOfRangeException(nameof(serviceUrl));
		    if (tenantId != null && !TenantIdRegex.IsMatch(tenantId))
				throw new ArgumentOutOfRangeException(nameof(tenantId));

			// Add slash for base url
			if (!serviceUrl.EndsWith("/"))
			   serviceUrl = serviceUrl + "/";

			// Save service url
			this.ServiceUrl = new Uri(serviceUrl);

			// Create http client
			this.HttpClient = new HttpClient() { BaseAddress = this.ServiceUrl };

			// Try to match tenant id from url if not supplied
		    this.TenantId = tenantId  ?? new[] {ProductionTenantUrlRegex, HostedTenantUrlRegex}
				.Select(i => i.Match(serviceUrl))
			    .Where(i => i.Success)
			    .Select(i => i.Groups["tenantid"].Value)
			    .FirstOrDefault();

			// Check result
			if (this.TenantId == null)
				throw new ArgumentOutOfRangeException(nameof(serviceUrl), "Could not infer tenant id, please supply it explicitly.");

			// Convert hex key
		    this.AdminKey = Enumerable.Range(0, adminKey.Length / 2).Select(x => Convert.ToByte(adminKey.Substring(x * 2, 2), 16)).ToArray();

			// Set admin user ID
		    this.AdminUserId = $"admin@{this.TenantId}.tresorit.io";
	    }

		/// <summary>
		/// Creates a new GET request object bound to this client
		/// </summary>
		/// <param name="path">Request path to use</param>
		/// <returns>Returns the new request</returns>
	    public AdminApiRequest CreateRequest(string path)
	    {
			return new AdminApiRequest(this, path);
	    }

		/// <summary>
		/// Creates a new POST request object bound to this client
		/// </summary>
		/// <param name="path">Request path to use</param>
		/// <returns>Returns the new request</returns>
		public AdminApiRequest CreatePostRequest(string path)
		{
			return this.CreateRequest(path).SetMethod(HttpMethod.Post);
		}

		/// <summary>
		/// Creates a new HEAD request object bound to this client
		/// </summary>
		/// <param name="path">Request path to use</param>
		/// <returns>Returns the new request</returns>
		public AdminApiRequest CreateHeadRequest(string path)
		{
			return this.CreateRequest(path).SetMethod(HttpMethod.Head);
		}

		/// <summary>
		/// Creates a new PUT request object bound to this client
		/// </summary>
		/// <param name="path">Request path to use</param>
		/// <returns>Returns the new request</returns>
		public AdminApiRequest CreatePutRequest(string path)
		{
			return this.CreateRequest(path).SetMethod(HttpMethod.Put);
		}

		/// <summary>
		/// Creates a new DELETE request object bound to this client
		/// </summary>
		/// <param name="path">Request path to use</param>
		/// <returns>Returns the new request</returns>
		public AdminApiRequest CreateDeleteRequest(string path)
		{
			return this.CreateRequest(path).SetMethod(HttpMethod.Delete);
		}

		/// <summary>
		/// Creates a new OPTIONS request object bound to this client
		/// </summary>
		/// <param name="path">Request path to use</param>
		/// <returns>Returns the new request</returns>
		public AdminApiRequest CreateOptionsRequest(string path)
		{
			return this.CreateRequest(path).SetMethod(HttpMethod.Options);
		}

		/// <summary>
		/// Creates a new TRACE request object bound to this client
		/// </summary>
		/// <param name="path">Request path to use</param>
		/// <returns>Returns the new request</returns>
		public AdminApiRequest CreateTraceRequest(string path)
		{
			return this.CreateRequest(path).SetMethod(HttpMethod.Trace);
		}

		/// <summary>
		/// Executes the given request and parses the received response
		/// </summary>
		/// <exception cref="AdminApiException">Thrown when the request resulted in an api
		/// error or the clietn was unable to parse the response</exception>
		/// <param name="request">Request to execute</param>
		/// <returns>Returns the response of the request</returns>
		protected internal async Task<AdminApiResponse> SendRequestAsync(HttpRequestMessage request)
		{
			// Check input
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			// Send request
			var response = await this.HttpClient.SendAsync(request);

			// Check status code, return response on success
			if (response.IsSuccessStatusCode)
				return new AdminApiResponse(response, await response.Content.ReadAsByteArrayAsync());

			// Try to parse error on failure
			AdminApiException apiException = null;
			try
			{
				// Parse JSON error
				apiException = JsonConvert.DeserializeObject<AdminApiException>(await response.Content.ReadAsStringAsync());
			}
			catch (Exception ex)
			{
				// Throw parsing error on failure
				apiException = new AdminApiException("ParsingError", "The client was unable to parse the error response message.", ex);
			}

			// Throw exception
			throw apiException;
		}
    }
}
