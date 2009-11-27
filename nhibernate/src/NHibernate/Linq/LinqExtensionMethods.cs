﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NHibernate.Linq
{
	public static class LinqExtensionMethods
	{
        public static void ForEach<T>(this IEnumerable<T> query, System.Action<T> method)
        {
            foreach (T item in query)
            {
                method(item);
            }
        }

        public static bool IsNullable(this System.Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

    }
}
