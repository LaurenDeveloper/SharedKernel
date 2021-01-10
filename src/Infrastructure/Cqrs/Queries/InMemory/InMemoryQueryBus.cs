﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Application.Cqrs.Queries;
using SharedKernel.Infrastructure.Cqrs.Middlewares;

namespace SharedKernel.Infrastructure.Cqrs.Queries.InMemory
{
    /// <summary>
    /// 
    /// </summary>
    public class InMemoryQueryBus : IQueryBus
    {
        private readonly ExecuteMiddlewaresService _executeMiddlewaresService;
        private readonly IServiceProvider _serviceProvider;
        private static readonly ConcurrentDictionary<Type, object> QueryHandlers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executeMiddlewaresService"></param>
        /// <param name="serviceProvider"></param>
        public InMemoryQueryBus(
            ExecuteMiddlewaresService executeMiddlewaresService,
            IServiceProvider serviceProvider)
        {
            _executeMiddlewaresService = executeMiddlewaresService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="query"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public async Task<TResponse> Ask<TResponse>(IQueryRequest<TResponse> query, CancellationToken cancellationToken)
        {
            await _executeMiddlewaresService.ExecuteAsync(query, cancellationToken);

            var handler = GetWrappedHandlers(query);

            if (handler == null)
                throw new QueryNotRegisteredException(query.ToString());

            return await handler.Handle(query, _serviceProvider, cancellationToken);
        }

        private QueryHandlerWrapper<TResponse> GetWrappedHandlers<TResponse>(IQueryRequest<TResponse> query)
        {
            Type[] typeArgs = { query.GetType(), typeof(TResponse) };

            var handlerType = typeof(IQueryRequestHandler<,>).MakeGenericType(typeArgs);
            var wrapperType = typeof(QueryHandlerWrapper<,>).MakeGenericType(typeArgs);

            var handlers =
                (IEnumerable)_serviceProvider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(handlerType));

            var wrappedHandlers = (QueryHandlerWrapper<TResponse>)QueryHandlers.GetOrAdd(query.GetType(), handlers.Cast<object>()
                .Select(_ => (QueryHandlerWrapper<TResponse>)Activator.CreateInstance(wrapperType)).FirstOrDefault());

            return wrappedHandlers;
        }
    }
}
