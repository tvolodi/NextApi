using System;
using System.Linq;
using System.Linq.Expressions;
using NextApi.Common.Ordering;

namespace NextApi.Server.Entity
{
    /// <summary>
    /// Order extensions
    /// </summary>
    public static class OrderExtensions
    {
        /// <summary>
        /// Generates ordering specified by the <see cref="Order"/> array
        /// </summary>
        /// <param name="source"></param>
        /// <param name="orders"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IQueryable<TEntity> GenerateOrdering<TEntity>(this IQueryable<TEntity> source, Order[] orders)
        {
            return orders.Aggregate(source, (current, order) => order.OrderOperator switch
            {
                OrderOperators.OrderBy => ApplyOrder(current, order.Property, "OrderBy"),
                OrderOperators.ThenBy => ApplyOrder(current, order.Property, "ThenBy"),
                OrderOperators.OrderByDescending => ApplyOrder(current, order.Property, "OrderByDescending"),
                OrderOperators.ThenByDescending => ApplyOrder(current, order.Property, "ThenByDescending"),
                _ => throw new ArgumentException(nameof(order.OrderOperator))
            });
        }
        
        // more info https://stackoverflow.com/questions/41244/dynamic-linq-orderby-on-ienumerablet-iqueryablet
        private static IOrderedQueryable<TEntity> ApplyOrder<TEntity>(
            IQueryable<TEntity> source, 
            string property, 
            string methodName) 
        {
            var props = property.Split('.');
            var type = typeof(TEntity);
            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach(string prop in props) {
                // use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TEntity), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);

            var result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                              && method.IsGenericMethodDefinition
                              && method.GetGenericArguments().Length == 2
                              && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), type)
                .Invoke(null, new object[] {source, lambda});
            return (IOrderedQueryable<TEntity>)result;
        }
    }
}
