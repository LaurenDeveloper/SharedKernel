﻿using System.Net;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace SharedKernel.Api.Middlewares
{
    /// <summary>
    /// Errors middleware
    /// </summary>
    public static class ErrorsMiddleware
    {
        /// <summary>
        /// Writes de exceptions to response
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseErrors(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(
                builder =>
                {
                    builder.Run(
                        async context =>
                        {
                            var error = context.Features.Get<IExceptionHandlerFeature>();

                            if (error != null)
                            {
                                if (error.Error.GetType() == typeof(AuthenticationException))
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                                }
                                else
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                                    await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                                }
                            }
                        });
                });

            return app;
        }
    }
}
