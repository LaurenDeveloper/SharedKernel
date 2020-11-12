﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SharedKernel.Application.Cqrs.Queries.Entities;
using SharedKernel.Domain.Aggregates;
using SharedKernel.Domain.Specifications.Common;

namespace SharedKernel.Application.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> AddFirst<T>(this IEnumerable<T> items, T item)
        {
            var itemsList = items == null ? new List<T>() : items.ToList();

            if (item != null)
                itemsList.Insert(0, item);

            return itemsList;
        }

        public static TimeSpan SumTimeSpan<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
        {
            return source.Select(selector).Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }

        /// <summary>
        ///     Convert a string to lambda expression
        ///     Example => "Person.Child.Name" : x => x.Person.Child.Name
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="column"></param>
        /// <returns></returns>
        public static dynamic ToLambdaExpression<TAggregate>(this string column)
        {
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentNullException(nameof(column));

            var props = column.Split('.');
            var type = typeof(TAggregate);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (var prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);

                if (pi == null)
                    throw new ArgumentNullException(prop);

                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TAggregate), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            return lambda;
        }

        public static bool AreConsecutive(this IEnumerable<int> elements)
        {
            return !elements.Select((i, j) => i - j).Distinct().Skip(1).Any();
        }

        #region Specifications

        public static IEnumerable<TAggregateRoot> AllMatching<TAggregateRoot>(
            this IEnumerable<TAggregateRoot> items, ISpecification<TAggregateRoot> spec)
            where TAggregateRoot : class, IAggregateRoot
        {
            return items.Where(spec.SatisfiedBy().Compile());
        }

        #endregion

        #region Random

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        private static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        #endregion Random

        public static IEnumerable<T> ContainsText<T>(this IEnumerable<T> query, string searchText)
        {
            Expression<Func<T, bool>> allExpressions = null;
            foreach (var property in typeof(T).GetProperties().Where(t => t.PropertyType == typeof(string)))
            {
                var exp = GetExpressionContainsString<T>(property.Name, searchText);
                allExpressions = allExpressions == null ? exp : allExpressions.Or(exp);
            }

            if (allExpressions != null)
                query = query.Where(allExpressions.Compile());

            return query;
        }

        public static IEnumerable<T> FilterContainsProperties<T>(this IEnumerable<T> query, IEnumerable<FilterProperty> properties)
        {
            if (properties == null)
                return query;

            query = properties.Where(f => !string.IsNullOrWhiteSpace(f.Value)).Aggregate(query,
                (current, filtro) => current.Where(GetExpressionContainsString<T>(filtro.Field, filtro.Value).Compile()));

            return query;
        }

        private static Expression<Func<T, bool>> GetExpressionContainsString<T>(string propertyName, string propertyValue)
        {
            var propertyInfo = typeof(T).GetProperties().SingleOrDefault(t => t.Name.ToUpper() == propertyName.ToUpper());
            if (propertyInfo == null)
                throw new Exception($"Propiedad {propertyName} no encontrada");

            // Expression
            var parameterExp = Expression.Parameter(typeof(T), "type");
            var propertyExp = Expression.Property(parameterExp, propertyName);

            // Method
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            if (method == null)
                throw new Exception("Método Contains no encontrado");

            // Value
            var someValue = Expression.Constant(propertyValue, typeof(string));

            var containsMethodExp = Expression.Call(propertyExp, method, someValue);
            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
        }
    }
}