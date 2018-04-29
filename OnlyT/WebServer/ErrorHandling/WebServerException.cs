namespace OnlyT.WebServer.ErrorHandling
{
    using System;

    public class WebServerException : Exception
    {
        public WebServerErrorCode Code { get; }

        public WebServerException(WebServerErrorCode code)
            : base(WebServerErrorCodes.GetDescription(code))
        {
            Code = code;
        }
    }
}
