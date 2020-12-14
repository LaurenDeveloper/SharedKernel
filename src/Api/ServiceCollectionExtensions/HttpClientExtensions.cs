﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.Api.ServiceCollectionExtensions
{
    /// <summary>
    /// Http client extensions
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Add http client with bearer token
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IServiceCollection AddClient<TClient>(this IServiceCollection services, IConfiguration configuration, string section) where TClient : class
        {
            services.AddHttpClient<TClient>(section)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration.GetSection(section).Value))
                .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();

            return services;
        }
    }

    internal class HttpClientAuthorizationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientAuthorizationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext == null)
                return await base.SendAsync(request, cancellationToken);


            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorizationHeader))
                request.Headers.Add("Authorization", new List<string> { authorizationHeader });

            const string accessToken = "access_token";

            var token = await _httpContextAccessor.HttpContext.GetTokenAsync(accessToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
