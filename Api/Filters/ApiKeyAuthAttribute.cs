using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace UretimTakipSistemi.Api.Filters
{
    public class ApiKeyAuthAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var expected = ConfigurationManager.AppSettings["ApiKey"];

            if (!actionContext.Request.Headers.TryGetValues("X-API-KEY", out var values))
            {
                actionContext.Response = actionContext.Request
                    .CreateResponse(HttpStatusCode.Unauthorized, "X-API-KEY header yok");
                return;
            }

            if (values.FirstOrDefault() != expected)
            {
                actionContext.Response = actionContext.Request
                    .CreateResponse(HttpStatusCode.Unauthorized, "API KEY hatalı");
            }
        }
    }
}
