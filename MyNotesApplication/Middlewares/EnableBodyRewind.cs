using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace MyNotesApplication.Middlewares
{
    public class EnableBodyRewind : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var bodyStr = "";
            var req = context.HttpContext.Request;

            // Allows using several time the stream in ASP.Net Core
            req.EnableBuffering();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (StreamReader reader
                      = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEnd();
            }

            // Rewind, so the core is not lost when it looks at the body for the request
            req.Body.Position = 0;

            // Do whatever works with bodyStr here
        }
    }
}
