using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;

namespace SpidNetSdk.OidConnect.Browsers
{
    public class RedirectBrowser : IBrowser
    {
        public Task<BrowserResult> InvokeAsync(BrowserOptions options)
        {
            var debug = options.EndUrl;
            return null;
        }
    }
}