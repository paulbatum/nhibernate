using NHibernate.Collection;
using NHibernate.Engine;

namespace NHibernate.Loader
{
	/// <summary>
	/// An interface for collection loaders
	/// </summary>
	public interface ICollectionInitializer
	{
		/// <summary>
		/// Initialize the given collection
		/// </summary>
		/// <param name="id"></param>
		/// <param name="collection"></param>
		/// <param name="owner"></param>
		/// <param name="session"></param>
		void Initialize( object id, PersistentCollection collection, object owner, ISessionImplementor session );
	}
}