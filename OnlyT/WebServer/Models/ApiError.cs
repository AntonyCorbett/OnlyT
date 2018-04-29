namespace OnlyT.WebServer.Models
{
    using ErrorHandling;
    using Newtonsoft.Json;

    public class ApiError
    {
        [JsonProperty(PropertyName = "errorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "conflictingId", NullValueHandling = NullValueHandling.Ignore)]
        public string ConflictingId { get; set; }

        public ApiError()
        {
        }

        public ApiError(WebServerErrorCode code, string conflictingId = null)
        {
            ErrorCode = (int)code;
            ErrorMessage = WebServerErrorCodes.GetDescription(code);
            ConflictingId = conflictingId;
        }
    }
}
