using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tresorit.ZeroKit.AdminApiClient
{
	/// <summary>
	/// Represents a request to the ZeroKit admin API
	/// </summary>
	/// <remarks>
	/// The request objects can be modified and reused multiple times
	/// </remarks>
	public class AdminApiRequest
	{
		/// <summary>
		/// List of headers which are have to be signed
		/// </summary>
		protected static readonly string[] HeadersToSign =
		{
			"UserId",
			"TresoritDate",
			"Content-SHA256",
			"Content-Type",
			"HMACHeaders"
		};

		/// <summary>
		/// Owner client of the request 
		/// </summary>
		public readonly AdminApiClient Client;

		/// <summary>
		/// Gets the HTTP method of the request
		/// </summary>
		public HttpMethod Method { get; protected set; }

		/// <summary>
		/// Internal representiation of the query parameters
		/// </summary>
		protected Dictionary<string, List<string>> InternalQueryParamaters;

		/// <summary>
		/// Internal representation of the headers
		/// </summary>
		protected Dictionary<string, List<string>> InternalHeaders;

		/// <summary>
		/// Request path
		/// </summary>
		public readonly string RequestPath;

		/// <summary>
		/// Gets the content bytes
		/// </summary>
		public byte[] ContentBytes { get; protected set; }

		/// <summary>
		/// Gets the header values of the request
		/// </summary>
		public IReadOnlyDictionary<string, string[]> Headers => new ReadOnlyDictionary<string, string[]>(this.InternalHeaders.ToDictionary(i => i.Key, i => i.Value.ToArray()));

		/// <summary>
		/// Gets the query parameters of the request
		/// </summary>
		public IReadOnlyDictionary<string, string[]> QueryParamaters => new ReadOnlyDictionary<string, string[]>(this.InternalQueryParamaters.ToDictionary(i => i.Key, i => i.Value.ToArray()));

		/// <summary>
		/// Gets the assembled query string o fthe request
		/// </summary>
		public string QueryString => string.Join("&", this.InternalQueryParamaters.SelectMany(n => n.Value.Select(i => i != null ? $"{n.Key}={i}" : n.Key)));

		/// <summary>
		/// Gets the relative request url assembled from the path and the query paramaters
		/// </summary>
		public Uri Url
		{
			get
			{
				// Build path with query
				var query = this.QueryString;
				var pathAndQuery = this.RequestPath.TrimStart('/');

				// Add or append query string
				if (query.Length > 0)
				{
					if (pathAndQuery.Contains("?"))
						pathAndQuery += query;
					else
						pathAndQuery += $"?{query}";
				}

				return new Uri(pathAndQuery, UriKind.Relative);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AdminApiRequest"/> class
		/// </summary>
		/// <param name="client">Owner client</param>
		/// <param name="path">Request path</param>
		internal AdminApiRequest(AdminApiClient client, string path)
		{
			Client = client;
			this.Method = HttpMethod.Get;
			this.InternalQueryParamaters = new Dictionary<string, List<string>>();
			this.InternalHeaders = new Dictionary<string, List<string>>();
			this.RequestPath = path;
			this.ContentBytes = new byte[0];
		}

		/// <summary>
		/// Adds a header value to the request headers
		/// </summary>
		/// <param name="name">Header name</param>
		/// <param name="value">Header value</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest AddHeader(string name, string value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (!this.InternalHeaders.ContainsKey(name))
				this.InternalHeaders[name] = new List<string>();

			this.InternalHeaders[name].Add(value);

			return this;
		}

		/// <summary>
		/// Sets the value of the given header to the given string
		/// </summary>
		/// <param name="name">Name of the header</param>
		/// <param name="value">Value of the header</param>
		/// <remarks>
		/// If the header value is null, the header will be removed
		/// </remarks>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest SetHeader(string name, string value)
		{
			this.InternalHeaders[name] = new List<string>();

			if (value == null)
				return this;

			return this.AddHeader(name, value);
		}

		/// <summary>
		/// Removes the given header
		/// </summary>
		/// <param name="name">Name of the header to remove</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest RemoveHeader(string name)
		{
			return this.SetHeader(name, null);
		}

		/// <summary>
		/// Removes the given header value
		/// </summary>
		/// <param name="name">Name of the header to modify</param>
		/// <param name="value">Value to remove</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest RemoveHeader(string name, string value)
		{
			if (this.InternalHeaders.ContainsKey(name))
				this.InternalHeaders[name] = this.InternalHeaders[name].Where(i => i != value).ToList();

			if (this.InternalHeaders[name].Count == 0)
				this.InternalHeaders.Remove(name);

			return this;
		}

		/// <summary>
		/// Adds a query parameter to the request
		/// </summary>
		/// <param name="name">Parameter name</param>
		/// <param name="value">Parameter value</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest AddQueryParamater(string name, string value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (!this.InternalQueryParamaters.ContainsKey(name))
				this.InternalQueryParamaters[name] = new List<string>();

			this.InternalQueryParamaters[name].Add(value);

			return this;
		}

		/// <summary>
		/// Sets the value of the given query parameter to the given string
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		/// <remarks>
		/// If the parameter value is null, the parameter wil be removed
		/// </remarks>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest SetQueryParamater(string name, string value)
		{
			this.InternalQueryParamaters[name] = new List<string>();

			if (value == null)
				return this;

			return this.AddQueryParamater(name, value);
		}

		/// <summary>
		/// Removes the given query parameter
		/// </summary>
		/// <param name="name">Name of the parameter to remove</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest RemoveQueryParamater(string name)
		{
			return this.SetQueryParamater(name, null);
		}

		/// <summary>
		/// Removes the given query parameter value
		/// </summary>
		/// <param name="name">Name of the parameter to modify</param>
		/// <param name="value">Value to remove</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest RemoveQueryParamater(string name, string value)
		{
			if (this.InternalQueryParamaters.ContainsKey(name))
				this.InternalQueryParamaters[name] = this.InternalQueryParamaters[name].Where(i => i != value).ToList();

			if (this.InternalQueryParamaters[name].Count == 0)
				this.InternalQueryParamaters.Remove(name);

			return this;
		}

		/// <summary>
		/// Sets the HTTP method of the request
		/// </summary>
		/// <param name="method">HTTP method to use</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest SetMethod(HttpMethod method)
		{
			this.Method = method;

			return this;
		}


		/// <summary>
		/// Sets the contents of the request
		/// </summary>
		/// <remarks>
		/// This method does not create a deep copy of the given array
		/// </remarks>
		/// <param name="content">Content bytes to use</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest SetContents(byte[] content)
		{
			this.ContentBytes = content ?? new byte[0];
			this.SetHeader("Content-SHA256", Sha256(this.ContentBytes));
			this.SetHeader("Content-Length", this.ContentBytes.Length.ToString());

			return this;
		}

		/// <summary>
		/// Sets the contents of the request with UTF-8 encoding
		/// </summary>
		/// <param name="content">String contents to use</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest SetContents(string content)
		{
			return this.SetContents(Encoding.UTF8.GetBytes(content ?? ""));
		}

		/// <summary>
		/// Sets the contents of the request
		/// </summary>
		/// <remarks>
		/// This method immedately reads the contents of the given stream.
		/// </remarks>
		/// <param name="content">Contents to use</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest SetContents(Stream content)
		{
			if (content == null)
				return this.SetContents(new byte[0]);

			var buffer = new MemoryStream();
			content.CopyTo(buffer);

			return this.SetContents(buffer.ToArray());
		}

		/// <summary>
		/// Sets the contents of the request
		/// </summary>
		/// <remarks>
		/// This method takes the given object's JSON serialized form and sets it as contents.
		/// The content-type of the request is also set to "application/json".
		/// </remarks>
		/// <param name="content">Contents to use</param>
		/// <returns>Returns the modified request</returns>
		public AdminApiRequest SetJsonContents(object content)
		{
			this.SetHeader("Content-Type", "application/json");

			if (content == null)
				return this.SetContents(new byte[0]);

			return this.SetContents(JsonConvert.SerializeObject(content));
		}

		/// <summary>
		/// Executes the request asynchronously with the given contents
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <param name="contents">String contents to use</param>
		/// <returns>Returns the response object</returns>
		public Task<AdminApiResponse> QueryAsync(string contents)
		{
			return this.SetContents(contents).
				QueryAsync();
		}

		/// <summary>
		/// Executes the request asynchronously with the given contents
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <param name="contents">Binary contents to use</param>
		/// <returns>Returns the response object</returns>
		public Task<AdminApiResponse> QueryAsync(byte[] contents)
		{
			return this.SetContents(contents).
				QueryAsync();
		}

		/// <summary>
		/// Executes the request asynchronously with the given contents
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <param name="contents">Stream contents to use</param>
		/// <returns>Returns the response object</returns>
		public Task<AdminApiResponse> QueryAsync(Stream contents)
		{
			return this.SetContents(contents).
				QueryAsync();
		}

		/// <summary>
		/// Executes the request asynchronously with the given contents
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <param name="contents">JSON contents to use</param>
		/// <returns>Returns the response object</returns>
		public Task<AdminApiResponse> QueryJsonAsync(object contents)
		{
			return this.SetJsonContents(contents).
				QueryAsync();
		}

		/// <summary>
		/// Executes the request asynchronously
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <returns>Returns the response object</returns>
		public Task<AdminApiResponse> QueryAsync()
		{
			// Sign request
			this.Sign();

			// Create request
			var request = new HttpRequestMessage(this.Method, this.Url);

			// Add contents
			request.Content = new ByteArrayContent(this.ContentBytes);

			// Add headers
			foreach (var header in this.InternalHeaders)
			{
				if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value) &&
				    !request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value))
					throw new Exception("Failed to compose request from the provided information!");
			}

			return this.Client.SendRequestAsync(request);
		}

		/// <summary>
		/// Executes the request synchronously
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <returns>Returns the response object</returns>
		public AdminApiResponse Query()
		{
			try
			{
				return this.QueryAsync().Result;
			}
			catch (AggregateException ae)
			{
				// Capture and throw original exception
				ExceptionDispatchInfo.Capture(ae.InnerException).Throw();

				// Code never reaches this point, but compiler needs it
				throw;
			}
		}

		/// <summary>
		/// Executes the request synchronously with the given contents
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <param name="contents">String contents to use</param>
		/// <returns>Returns the response object</returns>
		public AdminApiResponse Query(string contents)
		{
			return this.SetContents(contents).
				Query();
		}

		/// <summary>
		/// Executes the request synchronously with the given contents
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <param name="contents">Binary contents to use</param>
		/// <returns>Returns the response object</returns>
		public AdminApiResponse Query(byte[] contents)
		{
			return this.SetContents(contents).
				Query();
		}

		/// <summary>
		/// Executes the request synchronously with the given contents
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <param name="contents">Stream contents to use</param>
		/// <returns>Returns the response object</returns>
		public AdminApiResponse Query(Stream contents)
		{
			return this.SetContents(contents).
				Query();
		}

		/// <summary>
		/// Executes the request synchronously with the given contents
		/// </summary>
		/// <remarks>
		/// Warning: this method may throw exceptions on failure!
		/// </remarks>
		/// <exception cref="AdminApiException">Thrown when the request results in an api error</exception>
		/// <param name="contents">JSON contents to use</param>
		/// <returns>Returns the response object</returns>
		public AdminApiResponse QueryJson(object contents)
		{
			return this.SetJsonContents(contents).
				Query();
		}

		/// <summary>
		/// Signs the actual state of the request and automatically sets the required headers
		/// </summary>
		/// <remarks>
		/// The signature is valid only for 15 minutes!
		/// </remarks>
		/// <returns>Returns the signed request</returns>
		public AdminApiRequest Sign()
		{
			// Set obligatory headers
			this.SetHeader("UserId", this.Client.AdminUserId);
			this.SetHeader("Content-SHA256", Sha256(this.ContentBytes));
			this.SetHeader("Content-Length", this.ContentBytes.Length.ToString());
			this.SetHeader("TresoritDate", DateTimeOffset.UtcNow.ToString("s") + "Z");

			// Set header if not set already
			if (!this.InternalHeaders.ContainsKey("Content-Type"))
				this.SetHeader("Content-Type", "application/json");

			// Add HMAC header list
			this.SetHeader("HMACHeaders", string.Join(",", HeadersToSign));

			// Canonicalize request
			var stringToSign = $"{this.Method}\n" +
			                   $"{(this.Url.IsAbsoluteUri ? this.Url.PathAndQuery : this.Url.OriginalString).TrimStart('/')}\n" +
			                   string.Join("\n", HeadersToSign.Select(i => $"{i}:{this.InternalHeaders[i].Single()}"));

			// Compute signature
			var signature = HmacSha256(Encoding.UTF8.GetBytes(stringToSign), this.Client.AdminKey);

			// Set signature
			this.SetHeader("Authorization", $"AdminKey {signature}");

			return this;
		}

		/// <summary>
		/// Internal helper method to compute the Sha256 hash of the given byte array
		/// </summary>
		/// <param name="data">The data to compute the hash code for</param>
		/// <returns>Returns the hash value as a hexadecimal string</returns>
		protected static string Sha256(byte[] data)
		{
			if (data == null || data.Length == 0)
				return "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

			using (var sha256 = new System.Security.Cryptography.SHA256Managed())
			{
				return string.Join("", sha256.ComputeHash(data).Select(i => i.ToString("x2")));
			}
		}

		/// <summary>
		/// Internal helper method to compute the HMACSHA256 signature of the given data
		/// </summary>
		/// <param name="data"> The data to compute the hash code for</param>
		/// <param name="key">Key to use</param>
		/// <returns>Returns the computed hash as a base64 string</returns>
		protected static string HmacSha256(byte[] data, byte[] key)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (key.Length != 32)
				throw new ArgumentOutOfRangeException(nameof(key));

			using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(key))
			{
				return Convert.ToBase64String(hmacsha256.ComputeHash(data));
			}
		}
	}
}
