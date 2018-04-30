namespace OnlyT.WebServer.ErrorHandling
{
    // note the gaps in ID values are to retain compatibility with the SoundBox API

    public enum WebServerErrorCode
    {
        Success = 0,
        
        TimerDoesNotExist = 1129,
        UriTooManySegments,
        UriTooFewSegments,
        CouldNotIdentifyTimer,
        
        BadPrefix = 1134,
        BadHttpVerb,
        
        CouldNotTransitionTimer = 1137,
        ApiVersionNotSupported,
        NotAvailableInApiVersion,
        BadApiCode,

        ApiNotEnabled = 1151,

        SubscriptionAddressNotFound = 1155,
        SubscriptionPortNotSpecified,
        Throttled,

        UnknownError = 5128
    }
}
