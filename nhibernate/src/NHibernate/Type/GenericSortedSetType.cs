#if NET_2_0

using System;
using System.Collections.Generic;
using System.Reflection;

using Iesi.Collections.Generic;

using NHibernate.Collection;
using NHibernate.Collection.Generic;
using NHibernate.Engine;
using NHibernate.Util;
using NHibernate.Persister.Collection;

namespace NHibernate.Type
{
	/// <summary>
	/// An <see cref="IType"/> that maps a sorted <see cref="ISet&lt;T&gt;"/> collection
	/// to the database.
	/// </summary>
	public class GenericSortedSetType<T> : GenericSetType<T>
	{
		private IComparer<T> comparer;

		/// <summary>
		/// Initializes a new instance of a <see cref="GenericSortedSetType"/> class for
		/// a specific role.
		/// </summary>
		/// <param name="role">The role the persistent collection is in.</param>
		public GenericSortedSetType( string role, string propertyRef, IComparer<T> comparer )
			: base( role, propertyRef )
		{
			this.comparer = comparer;
		}

		public IComparer<T> Comparer
		{
			get { return comparer; }
		}

		public override object Instantiate()
		{
			return new SortedSet<T>( comparer );
		}
	}
}
#endif