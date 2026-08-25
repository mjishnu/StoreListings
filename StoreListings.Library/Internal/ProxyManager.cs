using System.Net;

namespace StoreListings.Library.Internal;

/// <summary>
/// App-side hook for routing the library's shared Store/FE3 HttpClients through a
/// user-configured proxy. The clients are lazily created singletons, so the proxy
/// must be set BEFORE first use; Raven initializes ProxyService at startup which
/// satisfies that, and later switches rebuild the handlers in place.
/// </summary>
public static class ProxyManager
{
    private static IWebProxy? _proxy;

    /// <summary>Sets the proxy used by the shared Store and FE3 clients. Null = direct.</summary>
    public static void SetProxy(IWebProxy? proxy)
    {
        _proxy = proxy;
        Helpers.ApplyProxy(proxy);
    }

    public static IWebProxy? GetProxy() => _proxy;
}
