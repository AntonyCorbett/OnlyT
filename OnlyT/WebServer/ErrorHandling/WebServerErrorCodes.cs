namespace OnlyT.WebServer.ErrorHandling
{
    using System.Net;

    public static class WebServerErrorCodes
    {
        public static string GetDescription(WebServerErrorCode code)
        {
            return code switch
            {
                WebServerErrorCode.Success => "Success",
                WebServerErrorCode.TimerDoesNotExist => "Timer does not exist",
                WebServerErrorCode.UriTooManySegments => "Malformed URI",
                WebServerErrorCode.UriTooFewSegments => "Malformed URI",
                WebServerErrorCode.BadPrefix => "Malformed URI",
                WebServerErrorCode.BadHttpVerb => "Wrong http method used",
                WebServerErrorCode.ApiVersionNotSupported => "API version not supported",
                WebServerErrorCode.NotAvailableInApiVersion => "Not available in selected API version",
                WebServerErrorCode.BadApiCode => "Invalid API code",
                WebServerErrorCode.ApiNotEnabled => "API is not enabled",
                WebServerErrorCode.SubscriptionAddressNotFound => "Subscription address not found",
                WebServerErrorCode.SubscriptionPortNotSpecified => "Subscription port not found",
                WebServerErrorCode.BadRequestBody => "Invalid request body",
                WebServerErrorCode.TimerNotEditable => "Timer is not editable",
                WebServerErrorCode.Throttled => "Client throttled",
                // ReSharper disable once RedundantCaseLabel
                WebServerErrorCode.UnknownError => "Unknown error",
                _ => "Unknown error"
            };
        }

        public static HttpStatusCode GetHttpErrorCode(WebServerErrorCode code)
        {
            return code switch
            {
                WebServerErrorCode.TimerDoesNotExist => HttpStatusCode.NotFound,
                WebServerErrorCode.UriTooManySegments => HttpStatusCode.BadRequest,
                WebServerErrorCode.UriTooFewSegments => HttpStatusCode.BadRequest,
                WebServerErrorCode.BadPrefix => HttpStatusCode.BadRequest,
                WebServerErrorCode.ApiVersionNotSupported => HttpStatusCode.BadRequest,
                WebServerErrorCode.NotAvailableInApiVersion => HttpStatusCode.BadRequest,
                WebServerErrorCode.SubscriptionAddressNotFound => HttpStatusCode.BadRequest,
                WebServerErrorCode.SubscriptionPortNotSpecified => HttpStatusCode.BadRequest,
                WebServerErrorCode.BadRequestBody => HttpStatusCode.BadRequest,
                WebServerErrorCode.TimerNotEditable => HttpStatusCode.Forbidden,

                // should be 429 but not yet available in HttpStatusCode
                WebServerErrorCode.Throttled => HttpStatusCode.ServiceUnavailable,
                WebServerErrorCode.UnknownError => HttpStatusCode.InternalServerError,
                WebServerErrorCode.BadHttpVerb => HttpStatusCode.MethodNotAllowed,
                WebServerErrorCode.BadApiCode => HttpStatusCode.Unauthorized,
                WebServerErrorCode.ApiNotEnabled => HttpStatusCode.Unauthorized,
                // ReSharper disable once RedundantCaseLabel
                WebServerErrorCode.Success => HttpStatusCode.OK,
                _ => HttpStatusCode.OK
            };
        }
    }
}
