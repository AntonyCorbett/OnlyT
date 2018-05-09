namespace OnlyT.WebServer.ErrorHandling
{
    // note the gaps in ID values are to retain compatibility with the SoundBox API
    public enum WebServerErrorCode
    {
        /// <summary>
        /// No error.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The timer does not exist.
        /// </summary>
        TimerDoesNotExist = 1129,

        /// <summary>
        /// The uri has too many segments.
        /// </summary>
        UriTooManySegments,

        /// <summary>
        /// The uri has too few segments.
        /// </summary>
        UriTooFewSegments,

        /// <summary>
        /// There is a bad prefix.
        /// </summary>
        BadPrefix = 1134,

        /// <summary>
        /// There is a bad http verb.
        /// </summary>
        BadHttpVerb,

        /// <summary>
        /// The api version not supported.
        /// </summary>
        ApiVersionNotSupported = 1138,

        /// <summary>
        /// Not available in this api version.
        /// </summary>
        NotAvailableInApiVersion,

        /// <summary>
        /// A bad api code was specified.
        /// </summary>
        BadApiCode,

        /// <summary>
        /// The api is not enabled.
        /// </summary>
        ApiNotEnabled = 1151,

        /// <summary>
        /// The subscription address was not found.
        /// </summary>
        SubscriptionAddressNotFound = 1155,

        /// <summary>
        /// The subscription port was not specified.
        /// </summary>
        SubscriptionPortNotSpecified,

        /// <summary>
        /// The request failed because of api throttling.
        /// </summary>
        Throttled,

        /// <summary>
        /// An unknown error.
        /// </summary>
        UnknownError = 5128
    }
}
