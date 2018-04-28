using System;

namespace OnlyT.WebServer.ErrorHandling
{
    public class WebServerException : Exception
    {
        private readonly WebServerErrorCode _code;
        public WebServerErrorCode Code => _code;

        public WebServerException(WebServerErrorCode code)
            : base(WebServerErrorCodes.GetDescription(code))
        {
            _code = code;
        }
    }
}
