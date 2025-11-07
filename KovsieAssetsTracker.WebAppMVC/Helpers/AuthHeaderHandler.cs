using System.Net.Http.Headers;

namespace KovsieAssetsTracker.WebAppMVC.Helpers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly SessionManager sessionManager;

        public AuthHeaderHandler(SessionManager _sessionManager)
        {
            sessionManager = _sessionManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = sessionManager.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
