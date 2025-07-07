using Api_Ordenes.Middlewares.ErrorMiddlewares;

namespace Api_Ordenes.Middlewares
{
    public static class InstallHandlerMiddleware
    {
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
