namespace OnlyT.WebServer.ErrorHandling
{
    using System;

#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class WebServerException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    {
        public WebServerException(WebServerErrorCode code)
            : base(WebServerErrorCodes.GetDescription(code))
        {
            Code = code;
        }

        public WebServerErrorCode Code { get; }
    }
}
