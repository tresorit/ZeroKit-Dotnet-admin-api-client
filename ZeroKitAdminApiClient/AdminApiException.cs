using System;
using Newtonsoft.Json;

namespace Tresorit.ZeroKit.AdminApiClient
{
    /// <summary>
    /// Exception class which represents a ZeroKit admin api error
    /// </summary>
    [JsonObject]
    public class AdminApiException : Exception
    {
        /// <summary>
        /// Admin api error code
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminApiException"/> class
        /// </summary>
        /// <param name="errorCode">API error code</param>
        /// <param name="errorMessage">API error message</param>
        /// <param name="cause">Exception that caused this error [OPTIONAL]</param>
        [JsonConstructor]
        public AdminApiException(string errorCode, string errorMessage, Exception cause = null) : base(errorMessage, cause)
        {
            this.ErrorCode = errorCode;
        }
    }
}
