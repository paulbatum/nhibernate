using System.Collections;
using NHibernate.Collection;
using NHibernate.Engine;

namespace NHibernate.Type
{
	/// <summary></summary>
	public class MapType : PersistentCollectionType
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="role"></param>
		public MapType( string role ) : base( role )
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="persister"></param>
		/// <returns></returns>
		public override PersistentCollection Instantiate( ISessionImplementor session, CollectionPersister persister )
		{
			return new Map( session );
		}

		/// <summary></summary>
		public override System.Type ReturnedClass
		{
			get { return typeof( IDictionary ); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		public override ICollection GetElementsCollection( object collection )
		{
			return ( ( IDictionary ) collection ).Values;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		public override PersistentCollection Wrap( ISessionImplementor session, object collection )
		{
			return new Map( session, ( IDictionary ) collection );
		}
	}
}