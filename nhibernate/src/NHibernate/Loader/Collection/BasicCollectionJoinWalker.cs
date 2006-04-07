using System;
using System.Collections;

using Iesi.Collections;

using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Loader.Collection
{
	/// <summary>
	/// Walker for collections of values and many-to-many associations
	/// </summary>
	public class BasicCollectionJoinWalker : CollectionJoinWalker
	{
		private readonly IQueryableCollection collectionPersister;

		public BasicCollectionJoinWalker(
			IQueryableCollection collectionPersister,
			int batchSize,
			string subquery,
			ISessionFactoryImplementor factory,
			IDictionary enabledFilters )
			: base( factory, enabledFilters )
			string alias = GenerateRootAlias( collectionPersister.Role );
			WalkCollectionTree( collectionPersister, alias );
			IList allAssociations = new ArrayList();
			ArrayHelper.AddAll( allAssociations, associations );
			allAssociations.Add( new OuterJoinableAssociation(
			                     	collectionPersister.CollectionType,
			                     	null,
			                     	null,
			                     	alias,
			                     	JoinType.LeftOuterJoin,
			                     	Factory,
			                     	CollectionHelper.EmptyMap
			                     	) );
			InitPersisters( allAssociations, LockMode.None );
			InitStatementString( alias, batchSize, subquery );
		}
		private void InitStatementString(
			string alias,
			int batchSize,
			string subquery )
		{
			int joins = CountEntityPersisters( associations );
			int collectionJoins = CountCollectionPersisters( associations ) + 1;
			SqlStringBuilder whereString = WhereString(
				alias,
				collectionPersister.KeyColumnNames,
				collectionPersister.KeyType,
				subquery,
				batchSize
				);
			string filter = collectionPersister.FilterFragment( alias, EnabledFilters );
			if( collectionPersister.IsManyToMany )
			{
				{
					if( oja.JoinableType == associationType )
					{
						// we found it
						filter += collectionPersister.GetManyToManyFilterFragment(
							oja.RHSAlias,
							EnabledFilters
							);
					}
				}
			}
			whereString.Insert( 0, StringHelper.MoveAndToBeginning( filter ) );
			JoinFragment ojf = MergeOuterJoins( associations );
			SqlSelectBuilder select = new SqlSelectBuilder( Factory )
				.SetSelectClause(
					collectionPersister.SelectFragment( alias, CollectionSuffixes[ 0 ] ) +
					SelectString( associations )
				)
				.SetFromClause( collectionPersister.TableName, alias )
				.SetWhereClause( whereString.ToSqlString() )
				.SetOuterJoins(
				ojf.ToFromFragmentString,
				ojf.ToWhereFragmentString
				);
			select.SetOrderByClause( OrderBy( associations, collectionPersister.GetSQLOrderByString( alias ) ) );
//			if ( Factory.IsCommentsEnabled ) 
//			{
//				select.SetComment( "load collection " + collectionPersister.Role );
//			}

			SqlString = select.ToSqlString();
		}
		/// <summary>
		/// We can use an inner join for first many-to-many association
		/// </summary>
		protected JoinType GetJoinType(
			FetchMode config,
			String path,
			ISet visitedAssociations,
			string lhsTable,
			string[ ] lhsColumns,
			bool nullable,
			int currentDepth,
			Cascades.CascadeStyle cascadeStyle )
			JoinType joinType = base.GetJoinType(
				type,
				config,
				path,
				lhsTable,
				lhsColumns,
				nullable,
				currentDepth,
				cascadeStyle
				);
			//we can use an inner join for the many-to-many
			if( joinType == JoinType.LeftOuterJoin && "".Equals( path ) )
			{
				joinType = JoinType.InnerJoin;
			}
			return joinType;
		}
		public override string ToString()
		{
			return GetType().FullName + '(' + collectionPersister.Role + ')';
		}
}