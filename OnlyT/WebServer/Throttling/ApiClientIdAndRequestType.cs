namespace OnlyT.WebServer.Throttling
{
    internal struct ApiClientIdAndRequestType
    {
        public string ClientId { get; }

        public ApiRequestType RequestType { get; }

        public ApiClientIdAndRequestType(string clientId, ApiRequestType requestType)
        {
            ClientId = clientId;
            RequestType = requestType;
        }
    }
}
