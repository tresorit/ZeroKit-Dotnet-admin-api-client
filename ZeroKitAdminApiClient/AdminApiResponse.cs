using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Tresorit.ZeroKit.AdminApiClient
{
	/// <summary>
	/// Represents an API response from the ZeroKit service.
	/// </summary>
	public class AdminApiResponse
	{
		/// <summary>
		/// The underlying HTTP response object
		/// </summary>
		protected readonly HttpResponseMessage Response;

		/// <summary>
		/// The contents of the response read by the client
		/// </summary>
		protected readonly byte[] ContentBytes;

		/// <summary>
		/// Gets the response contents as a string
		/// </summary>
		/// <remarks>
		/// If the content is not a valid UTF8 text, this method may throw exceptions or returns invalid data.
		/// </remarks>
		public string AsString => this.ContentBytes == null ? null : Encoding.UTF8.GetString(this.ContentBytes);

		/// <summary>
		/// Gets the response contents as a byte array
		/// </summary>
		public byte[] AsBytes => this.ContentBytes;

		/// <summary>
		/// Gets the reponse status code
		/// </summary>
		public HttpStatusCode StatusCode => this.Response.StatusCode;

		/// <summary>
		/// Gets the response reason phrase
		/// </summary>
		public string ReasonPhrase => this.Response.ReasonPhrase;

		/// <summary>
		/// Gets the response headers
		/// </summary>
		public ReadOnlyDictionary<string, string[]> Headers => new ReadOnlyDictionary<string, string[]>(this.Response.Headers.ToDictionary(i => i.Key, i => i.Value.ToArray()));

		/// <summary>
		/// Gets the response contents as a parsed json object
		/// </summary>
		/// <typeparam name="T">Type of the target object</typeparam>
		/// <remarks>
		/// If the content is not a valid UTF8 text, or the text is not a valid JSON string,
		/// this method may throw exceptions or returns invalid data.
		/// </remarks>
		/// <returns>Returnes the parsed and converted JSON object</returns>
		public T AsJson<T>()
		{
			return JsonConvert.DeserializeObject<T>(this.AsString);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AdminApiResponse"/> class
		/// </summary>
		/// <param name="response">The underlying response object</param>
		/// <param name="bytes">The contents of the response read by the client</param>
		protected internal AdminApiResponse(HttpResponseMessage response, byte[] bytes)
		{
			Response = response;
			ContentBytes = bytes;
		}
	}
}
