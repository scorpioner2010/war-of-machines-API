using System.Net;

if ((args.Length != 1 && args.Length != 2) || !Uri.TryCreate(args[0], UriKind.Absolute, out Uri? uri))
{
    return 2;
}

try
{
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
    using var request = new HttpRequestMessage(HttpMethod.Get, uri);
    if (args.Length == 2)
        request.Headers.Host = args[1];
    using HttpResponseMessage response = await client.SendAsync(request);
    return response.StatusCode == HttpStatusCode.OK ? 0 : 1;
}
catch
{
    return 1;
}
