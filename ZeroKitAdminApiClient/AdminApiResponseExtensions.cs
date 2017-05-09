using System.Threading.Tasks;

namespace Tresorit.ZeroKit.AdminApiClient
{
	/// <summary>
	/// Extension methods for <see cref="AdminApiResponse"/> class
	/// </summary>
	public static class AdminApiResponseExtensions
	{
		/// <summary>
		/// Awaits the given response task and returns the respones contents as a string
		/// </summary>
		/// <remarks>
		/// This method creates quick and handy way for accessing response contents in a fluent way.
		/// Note: that this method should only be used when the response object itself is not needed anymore,
		/// and only its contents are important.
		/// </remarks>
		/// <param name="responseTask">Response task to await</param>
		/// <returns>Returns a task which which can be awaited for the string response contents</returns>
		public static async Task<string> AsStringAsync(this Task<AdminApiResponse> responseTask)
		{
			var response = await responseTask;

			return response.AsString;
		}

		/// <summary>
		/// Awaits the given response task and returns the respones contents as a byte array
		/// </summary>
		/// <remarks>
		/// This method creates quick and handy way for accessing response contents in a fluent way.
		/// Note: that this method should only be used when the response object itself is not needed anymore,
		/// and only its contents are important.
		/// </remarks>
		/// <param name="responseTask">Response task to await</param>
		/// <returns>Returns a task which which can be awaited for the binary response contents</returns>
		public static async Task<byte[]> AsBytesAsync(this Task<AdminApiResponse> responseTask)
		{
			var response = await responseTask;

			return response.AsBytes;
		}

		/// <summary>
		/// Awaits the given response task and returns the respones contents as a parsed JSON object
		/// </summary>
		/// <remarks>
		/// This method creates quick and handy way for accessing response contents in a fluent way.
		/// Note: that this method should only be used when the response object itself is not needed anymore,
		/// and only its contents are important.
		/// </remarks>
		/// <param name="responseTask">Response task to await</param>
		/// <returns>Returns a task which which can be awaited for the JSON response object</returns>
		public static async Task<T> AsJsonAsync<T>(this Task<AdminApiResponse> responseTask)
		{
			var response = await responseTask;

			return response.AsJson<T>();
		}
	}
}
