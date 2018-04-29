namespace OnlyT.WebServer.ErrorHandling
{
    using System.Net;

    public static class WebServerErrorCodes
    {
        public static string GetDescription(WebServerErrorCode code)
        {
            switch (code)
            {
                case WebServerErrorCode.Success:
                    return "Success";

                case WebServerErrorCode.TimerDoesNotExist:
                    return "Timer does not exist";

                case WebServerErrorCode.UriTooManySegments:
                case WebServerErrorCode.UriTooFewSegments:
                case WebServerErrorCode.BadPrefix:
                    return "Malformed URI";

                case WebServerErrorCode.CouldNotIdentifyTimer:
                    return "Could not identify timer";

                case WebServerErrorCode.BadHttpVerb:
                    return "Wrong http method used";

                case WebServerErrorCode.CouldNotTransitionTimer:
                    return "Could not transition timer";

                case WebServerErrorCode.ApiVersionNotSupported:
                    return "API version not supported";

                case WebServerErrorCode.NotAvailableInApiVersion:
                    return "Not available in selected API version";

                case WebServerErrorCode.BadApiCode:
                    return "Invalid API code";

                case WebServerErrorCode.ApiNotEnabled:
                    return "API is not enabled";

                case WebServerErrorCode.SubscriptionAddressNotFound:
                    return "Subscription address not found";

                case WebServerErrorCode.SubscriptionPortNotSpecified:
                    return "Subscription port not found";

                case WebServerErrorCode.UnknownError:
                default:
                    return "Unknown error";
            }
        }

        public static HttpStatusCode GetHttpErrorCode(WebServerErrorCode code)
        {
            switch (code)
            {
                case WebServerErrorCode.TimerDoesNotExist:
                    return HttpStatusCode.NotFound;

                case WebServerErrorCode.UriTooManySegments:
                case WebServerErrorCode.UriTooFewSegments:
                case WebServerErrorCode.CouldNotIdentifyTimer:
                case WebServerErrorCode.BadPrefix:
                case WebServerErrorCode.ApiVersionNotSupported:
                case WebServerErrorCode.NotAvailableInApiVersion:
                case WebServerErrorCode.SubscriptionAddressNotFound:
                case WebServerErrorCode.SubscriptionPortNotSpecified:
                    return HttpStatusCode.BadRequest;

                case WebServerErrorCode.UnknownError:
                    return HttpStatusCode.InternalServerError;

                case WebServerErrorCode.BadHttpVerb:
                    return HttpStatusCode.MethodNotAllowed;

                case WebServerErrorCode.CouldNotTransitionTimer:
                    return HttpStatusCode.Conflict;

                case WebServerErrorCode.BadApiCode:
                case WebServerErrorCode.ApiNotEnabled:
                    return HttpStatusCode.Unauthorized;

                case WebServerErrorCode.Success:
                default:
                    return HttpStatusCode.OK;
            }
        }
    }
}
