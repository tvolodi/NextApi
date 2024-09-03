using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace NextApi.Server.Entity
{
    /// <summary>
    /// Extensions for <see cref="object"/>.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Cached delegates.
        /// </summary>
        private static readonly ConcurrentDictionary<Tuple<Type, Type>, Func<object, object>> CastCache
            = new ConcurrentDictionary<Tuple<Type, Type>, Func<object, object>>();
        
        /// <summary>
        /// Create delegate, that casts object at runtime.
        /// </summary>
        /// <param name="from">From type.</param>
        /// <param name="to">To type.</param>
        /// <returns>Delegate to cast.</returns>
        private static Func<object, object> MakeCastDelegate(Type from, Type to)
        {
            var p = Expression.Parameter(typeof(object)); //do not inline
            return Expression.Lambda<Func<object, object>>(
                Expression.Convert(Expression.ConvertChecked(Expression.Convert(p, from), to), typeof(object)),
                p).Compile();
        }

        /// <summary>
        /// Get delegate, that casts from one type to another.
        /// </summary>
        /// <param name="from">From type.</param>
        /// <param name="to">To type.</param>
        /// <returns>Delegate to cast.</returns>
        private static Func<object, object> GetCastDelegate(Type from, Type to)
        {
            var key = new Tuple<Type, Type>(from, to);
            if (CastCache.TryGetValue(key, out var castDelegate)) return castDelegate;
            
            castDelegate = MakeCastDelegate(from, to);
            CastCache.TryAdd(key, castDelegate);
            return castDelegate;
        }

        /// <summary>
        /// Convert object to specific type.
        /// </summary>
        /// <param name="value">Value to convert from.</param>
        /// <param name="to">Type to convert to.</param>
        /// <returns>Converted value.</returns>
        public static object Cast(this object value, Type to)
            => GetCastDelegate(value.GetType(), to).Invoke(value);
    }
}