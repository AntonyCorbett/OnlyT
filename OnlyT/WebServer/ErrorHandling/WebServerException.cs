namespace OnlyT.WebServer.ErrorHandling
{
    using System;

    public class WebServerException : Exception
    {
        public WebServerException(WebServerErrorCode code)
            : base(WebServerErrorCodes.GetDescription(code))
        {
            Code = code;
        }

        public WebServerErrorCode Code { get; }
    }
}
