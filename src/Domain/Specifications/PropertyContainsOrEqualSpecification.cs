﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using SharedKernel.Domain.Specifications.Common;

namespace SharedKernel.Domain.Specifications
{
    public class PropertyContainsOrEqualSpecification<T> : Specification<T> where T : class
    {
        private PropertyInfo PropertyInfo { get; }

        private string Value { get; }

        public PropertyContainsOrEqualSpecification(PropertyInfo propertyInfo, string value)
        {
            PropertyInfo = propertyInfo;
            Value = value;
        }

        public override Expression<Func<T, bool>> SatisfiedBy()
        {
            var parameterExp = Expression.Parameter(typeof(T), "type");
            var propertyExp = Expression.Property(parameterExp, PropertyInfo.Name);

            var methodInfo = PropertyInfo.PropertyType == typeof(string)
                ? typeof(string).GetMethod("Contains", new[] { typeof(string) })
                : PropertyInfo.PropertyType.GetMethod("Equals", new[] { PropertyInfo.PropertyType });

            if (methodInfo == null)
                throw new Exception("Método no encontrado");

            // Valor propiedad
            object value;
            if (PropertyInfo.PropertyType != typeof(string))
                value = Convert.ChangeType(Value, PropertyInfo.PropertyType);
            else
                value = Value;

            var someValue = Expression.Constant(value, PropertyInfo.PropertyType);
            var containsMethodExp = Expression.Call(propertyExp, methodInfo, someValue);
            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
        }
    }
}