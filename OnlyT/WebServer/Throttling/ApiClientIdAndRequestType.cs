namespace OnlyT.WebServer.Throttling;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
internal readonly record struct ApiClientIdAndRequestType(string ClientId, ApiRequestType RequestType);
