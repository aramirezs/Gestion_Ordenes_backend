using Newtonsoft.Json;
using Domain.Models;
using System.Net;
using System.Security.Claims;

namespace Api_Ordenes.Middlewares.ErrorMiddlewares
{
    public sealed class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration configuration;

        public ErrorHandlerMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            this.configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ErrorHandler ex)
            {
                await ErrorHandlerAsync(context, ex);
            }
        }

        private async Task ErrorHandlerAsync(HttpContext context, Exception ex)
        {
            string message = null;

            context.Response.ContentType = "application/json";

            switch (ex)
            {
                case ErrorHandler eh:

                    context.Response.StatusCode = (int)eh.Code;

                    message = eh.Message;

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(MessageResult<object>.Of(message, eh.ExceptionData, eh.Status, eh.InternalResponse)));

                    break;

                case Exception e:

                    message = string.IsNullOrWhiteSpace(e.Message) ? "Error desconocido" : e.Message;

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(MessageResult<object>.Of(message, ex.Data, context.Response.StatusCode)));

                    break;
            }

        }
    }
}
