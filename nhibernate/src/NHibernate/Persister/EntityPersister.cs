using System;
using System.Text;
using System.Data;
using System.Collections;
using NHibernate.Util;
using NHibernate.Loader;
using NHibernate.Mapping;
using NHibernate.Hql;
using NHibernate.Sql;
using NHibernate.Type;
using NHibernate.Engine;
using NHibernate.Id;


namespace NHibernate.Persister {
	/// <summary>
	/// Default implementation of the <c>ClassPersister</c> interface. Implements the
	/// "table-per-class hierarchy" mapping strategy for an entity class
	/// </summary>
	public class EntityPersister : AbstractEntityPersister, IQueryable {
		private ISessionFactoryImplementor factory;

		private string qualifiedTableName;
		private string[] tableNames;

		private string deleteString;
		private string insertString;
		private string updateString;
		private string identityInsertString;

		private int[] propertyColumnSpans;
		private bool[] definedOnSubclass;
		private string[][] propertyColumnNames;
		private string[][] propertyColumnAliases;
		private string[][] subclassPropertyColumnNameClosure;

		private string discriminatorColumnName;
		private string[] subclassColumnClosure;
		private string[] subclassColumnAliasClosure;
		private IType[] subclassPropertyTypeClosure;
		private System.Type[] subclassClosure;
		private bool hasColumns;
		private OuterJoinLoaderType[] joinedFetch;

		private Hashtable subclassesByDiscriminatorValue = new Hashtable();

		private IDiscriminatorType discriminatorType;
		private string discriminatorSQLString;

		private Hashtable loaders = new Hashtable();
		private Hashtable lockers = new Hashtable();

		private string[] StringArray = {};
		private IType[] TypeArray = {};

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EntityPersister));

		public override void PostInstantiate(ISessionFactoryImplementor factory) {

			Hashtable mods = new Hashtable();
			foreach(DictionaryEntry e in typesByPropertyPath) {
				IType type = (IType) e.Value;
				if ( type.IsEntityType ) {
					string path = (string) e.Key;
					string[] columns = (string[]) columnNamesByPropertyPath[path];
					if ( columns.Length==0 ) columns = IdentifierColumnNames; //1-to-1 assoc
					EntityType etype = (EntityType) type;
					IType idType = factory.GetIdentifierType( etype.PersistentClass );

					string idpath = path + StringHelper.Dot + PathExpressionParser.EntityID;
					mods.Add(idpath, idType);
					columnNamesByPropertyPath.Add(idpath, columns);
					if ( idType.IsComponentType ) {
						IAbstractComponentType actype = (IAbstractComponentType) idType;
						string[] props = actype.PropertyNames;
						IType[] subtypes = actype.Subtypes;
						if ( actype.GetColumnSpan(factory) != columns.Length )
							throw new MappingException("broken mapping for: " + ClassName + StringHelper.Dot + path);
						int j=0;
						for (int i=0; i<props.Length; i++) {
							string subidpath = idpath + StringHelper.Dot + props[i];
							string[] componentColumns = new string[ subtypes[i].GetColumnSpan(factory) ];
							for (int k=0; k<componentColumns.Length; k++) {
								componentColumns[k] = columns[j++];
							}
							columnNamesByPropertyPath.Add(subidpath, componentColumns);
							mods.Add( subidpath, actype.Subtypes[i] );
						}
					}
				}
			}
			foreach(DictionaryEntry de in mods) {
				typesByPropertyPath.Add(de.Key, de.Value);
			}

			IUniqueEntityLoader loader = new EntityLoader(this, factory);

			loaders.Add( LockMode.None, loader );
			loaders.Add( LockMode.Read, loader );

			string selectForUpdate = factory.Dialect.SupportsForUpdate ?
				GenerateSelectForUpdateString() : GenerateSelectString();

			loaders.Add(
				LockMode.Upgrade,
				new SimpleEntityLoader ( this, selectForUpdate, LockMode.Upgrade )
				);

			string selectForUpdateNoWait = factory.Dialect.SupportsForUpdateNoWait ?
				GenerateSelectForUpdateNowaitString() : selectForUpdate;

			loaders.Add(
				LockMode.UpgradeNoWait,
				new SimpleEntityLoader( this, selectForUpdateNoWait, LockMode.UpgradeNoWait )
				);

		}

		public override bool IsDefinedOnSubclass(int i) {
			return definedOnSubclass[i];
		}

		public override string DiscriminatorColumnName {
			get { return discriminatorColumnName; }
		}

		public override OuterJoinLoaderType EnableJoinedFetch(int i) {
			return joinedFetch[i];
		}

		public override IType GetSubclassPropertyType(int i) {
			return subclassPropertyTypeClosure[i];
		}

		public override int CountSubclassProperties() {
			return subclassPropertyTypeClosure.Length;
		}

		public override string TableName {
			get { return qualifiedTableName; }
		}

		public override string[] GetSubclassPropertyColumnNames(int i) {
			return subclassPropertyColumnNameClosure[i];
		}

		public override string[] GetPropertyColumnNames(int i) {
			return propertyColumnAliases[i];
		}
		public override IDiscriminatorType DiscriminatorType {
			get { return discriminatorType; }
		}

		public override string DiscriminatorSQLString {
			get { return discriminatorSQLString; }
		}

		public System.Type[] SubclassClosure {
			get { return subclassClosure; }
		}
		public override System.Type GetSubclassForDiscriminatorValue(object value) {
			if (value==null) {
				return (System.Type) subclassesByDiscriminatorValue[ ObjectUtils.Null ];
			} else {
				return (System.Type) subclassesByDiscriminatorValue[ value ];
			}
		}

		public override object IdentifierSpace {
			get { return qualifiedTableName; }
		}

		public override object[] PropertySpaces {
			get { return tableNames; }
		}

		protected string SqlDelete {
			get { return deleteString; }
		}

		protected string SqlInsert {
			get { return insertString; }
		}

		protected string SqlIdentityInsert {
			get { return identityInsertString; }
		}

		protected string SqlUpdate {
			get { return updateString; }
		}

		protected string GenerateDeleteString() {
			return new Delete()
				.SetTableName( TableName )
				.SetPrimaryKeyColumnNames( IdentifierColumnNames )
				.SetVersionColumnName( VersionColumnName )
				.ToStatementString();
		}

		protected string GenerateInsertString(bool identityInsert, bool[] includeProperty) {
			Insert insert = new Insert(dialect)
				.SetTableName( TableName );
			for (int i=0; i<hydrateSpan; i++) {
				if ( includeProperty[i] ) insert.AddColumns( propertyColumnNames[i] );
			}
			if ( IsPolymorphic ) insert.AddColumn ( DiscriminatorColumnName, discriminatorSQLString );
			if (!identityInsert) {
				insert.AddColumns( IdentifierColumnNames );
			} else {
				insert.AddIdentityColumn( IdentifierColumnNames[0] );
			}
			return insert.ToStatementString();
		}

		/// <summary>
		/// Generate the SQL that selects a row by id using <c>FOR UPDATE</c>
		/// </summary>
		/// <returns></returns>
		protected string GenerateSelectForUpdateString() {
			return GenerateSelectString() + " for update";
		}

		/// <summary>
		/// Generate the SQL taht selects a row by id using <c>FOR UPDATE</c>
		/// </summary>
		/// <returns></returns>
		protected string GenerateSelectForUpdateNowaitString() {
			return GenerateSelectString() + " for update nowait";
		}

		protected string GenerateSelectString() {
			SimpleSelect select = new SimpleSelect()
				.SetTableName( TableName )
				.AddColumns( IdentifierColumnNames )
				.AddColumns( subclassColumnClosure, subclassColumnAliasClosure );
			if ( HasSubclasses ) select.AddColumn( DiscriminatorColumnName );
			return select.AddCondition( IdentifierColumnNames, "=?" ).ToStatementString();
		}

		protected string GenerateUpdateString(bool[] includeProperty) {
			Update update = new Update()
				.SetTableName( TableName )
				.SetPrimaryKeyColumnNames( IdentifierColumnNames )
				.SetVersionColumnName( VersionColumnName );
			for (int i=0; i<hydrateSpan; i++) {
				if ( includeProperty[i] ) update.AddColumns( propertyColumnNames[i] );
			}
			return update.ToStatementString();
		}

		protected string GenerateLockString() {
			SimpleSelect select = new SimpleSelect()
				.SetTableName( TableName )
				.AddColumn( IdentifierColumnNames[0])
				.AddCondition( IdentifierColumnNames, "=?" );
			if ( IsVersioned ) {
				select.AddWhereToken("and")
					.AddCondition( VersionColumnName, "=?" );
			}
			return select.ToStatementString();
		}

		/// <summary>
		/// Marshall the fields of a persistent instance to a prepared statement
		/// </summary>
		/// <param name="id"></param>
		/// <param name="fields"></param>
		/// <param name="includeProperty"></param>
		/// <param name="st"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		protected int Dehydrate(object id, object[] fields, bool[] includeProperty, IDbCommand st, ISessionImplementor session) {
			if (log.IsDebugEnabled ) log.Debug("Dehydrating entity: " + ClassName + '#' + id);

			int index = 1;
			for (int j=0; j<hydrateSpan; j++) {
				if ( includeProperty[j] ) {
					PropertyTypes[j].NullSafeSet( st, fields[j], index, session );
					index += propertyColumnSpans[j];
				}
			}

			if ( id!=null ) {
				IdentifierType.NullSafeSet( st, id, index, session );
				index += IdentifierColumnNames.Length;
			}

			return index;
		}

		/// <summary>
		/// Load an instance uing either the <c>forUpdateLoader</c> or the other joining <c>loader</c>,
		/// depending upon the value of the <c>lock</c> parameter
		/// </summary>
		/// <param name="id"></param>
		/// <param name="optionalObject"></param>
		/// <param name="lockMode"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		public override object Load(object id, object optionalObject, LockMode lockMode, ISessionImplementor session) {
			if ( log.IsDebugEnabled ) log.Debug( "Materializing entity: " + ClassName + '#' + id );

			return ( (IUniqueEntityLoader)loaders[lockMode]).Load(session, id, optionalObject);
		}

		/// <summary>
		/// Do a version check
		/// </summary>
		/// <param name="id"></param>
		/// <param name="version"></param>
		/// <param name="obj"></param>
		/// <param name="lockMode"></param>
		/// <param name="session"></param>
		public override void Lock(object id, object version, object obj, LockMode lockMode, ISessionImplementor session) {
			if (lockMode.GreaterThan(LockMode.None) ) {

				if (log.IsDebugEnabled ) {
					log.Debug("Locking entity: " + ClassName + '#' + id);
					if ( IsVersioned ) log.Debug("Version: " + version);
				}

				IDbCommand st = session.Batcher.PrepareStatement( (string) lockers[lockMode] );
				try {
					IdentifierType.NullSafeSet(st, id, 1, session);
					if ( IsVersioned ) VersionType.NullSafeSet(st, version, 2, session);

					IDataReader rs = st.ExecuteReader();
					try {
						if ( rs.Read() ) throw new StaleObjectStateException( MappedClass, id);
					} finally {
						rs.Close();
					}
				} finally {
					session.Batcher.CloseStatement(st);
				}
			}
		}

		/// <summary>
		/// Persist an object
		/// </summary>
		/// <param name="id"></param>
		/// <param name="fields"></param>
		/// <param name="obj"></param>
		/// <param name="session"></param>
		public override void Insert(object id, object[] fields, object obj, ISessionImplementor session) {
			if (log.IsDebugEnabled) {
				log.Debug("Inserting entity: " + ClassName + '#' + id);
				if ( IsVersioned ) log.Debug("Version: " + Versioning.GetVersion(fields, this));
			}

			// Render the SQL query
			IDbCommand statement = session.Batcher.PrepareBatchStatement( SqlInsert );

			try {

				// Write the values of the field onto the prepared statement - we MUST use the
				// state at the time the insert was issued (cos of foreign key constraints)
				// not necessarily the obect's current state

				Dehydrate(id, fields, PropertyInsertability, statement, session);

				session.Batcher.AddToBatch(1);
			} catch (Exception e ) {
				throw e;
			}
		}

		/// <summary>
		/// Persist an object, using a natively generated identifier
		/// </summary>
		/// <param name="fields"></param>
		/// <param name="obj"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		public override object Insert(object[] fields, object obj, ISessionImplementor session) {

			if (log.IsDebugEnabled) {
				log.Debug("Inserting entity: " + ClassName + " (native id)");
				if ( IsVersioned ) log.Debug( "Version: " + Versioning.GetVersion(fields, this) );
			}

			IDbCommand statement = session.Batcher.PrepareBatchStatement( SqlIdentityInsert );

			try {
				Dehydrate(null, fields, PropertyInsertability, statement, session);
				statement.ExecuteNonQuery();
			} catch (Exception e) {
				throw e;
			} finally {
				session.Batcher.CloseStatement(statement);
			}

			// fetch the generated id:
			IDbCommand idselect = session.Batcher.PrepareStatement( SqlIdentitySelect );

			try {
				IDataReader rs = idselect.ExecuteReader();
				object id;
				try {
					if ( !rs.Read() ) throw new HibernateException("The database returned no natively generated identity value");
					id = IdentifierGeneratorFactory.Get( rs, IdentifierType.ReturnedClass );
				} finally {
					rs.Close();
				}
				log.Debug("Natively generated identity: " + id);

				return id;
			} catch (Exception e) {
				throw e;
			} finally {
				session.Batcher.CloseStatement(idselect);
			}
		}

		/// <summary>
		/// Delete an object
		/// </summary>
		/// <param name="id"></param>
		/// <param name="version"></param>
		/// <param name="obj"></param>
		/// <param name="session"></param>
		public override void Delete(object id, object version, object obj, ISessionImplementor session) {
			
			if ( log.IsDebugEnabled ) {
				log.Debug("Deleting entity: " + ClassName + '#' + id);
				if ( IsVersioned ) log.Debug( "Version: " + version );
			}

			IDbCommand statement;
			if ( IsVersioned ) {
				statement = session.Batcher.PrepareStatement( SqlDelete );
			} else {
				statement = session.Batcher.PrepareBatchStatement( SqlDelete );
			}

			try {

				// Do the key. The key is immutable so we can use the current object state -
				// not necessarily the state at the time the delete was issued

				IdentifierType.NullSafeSet( statement, id, 1, session );

				// We should use the current object state

				if ( IsVersioned ) {
					VersionType.NullSafeSet( statement, version, IdentifierColumnNames.Length + 1, session );
					Check( statement.ExecuteNonQuery(), id );
				} else {
					session.Batcher.AddToBatch(1);
				}
			} catch (Exception e) {
				throw e;
			} finally {
				if ( IsVersioned ) session.Batcher.CloseStatement(statement);
			}
		}

		public override void Update(object id, object[] fields, int[] dirtyFields, object oldVersion, object obj, ISessionImplementor session) {
			if (UseDynamicUpdate && dirtyFields!=null ) {
				bool[] propsToUpdate = new bool[hydrateSpan];
				for (int i=0; i<hydrateSpan; i++) {
					bool dirty = false;
					for (int j=0; j<dirtyFields.Length; j++) {
						if ( dirtyFields[j]==i ) dirty=true;
					}
					propsToUpdate[i] = dirty || VersionProperty==i;
				}
				Update(id, fields, propsToUpdate, oldVersion, obj, GenerateUpdateString(propsToUpdate), session);
			} else {
				Update(id, fields, PropertyUpdateability, oldVersion, obj, SqlUpdate, session);
			}
		}

		protected void Update(object id, object[] fields, bool[] includeProperty, object oldVersion, object obj, string sql, ISessionImplementor session) {
			if (log.IsDebugEnabled ) {
				log.Debug("Updating entity: " + ClassName + '#' + id);
				if ( IsVersioned ) log.Debug( "Existing version: " + oldVersion + " -> New Version: " + fields[ VersionProperty ] );
			}

			if (!hasColumns) return;

			IDbCommand statement;
			if (IsVersioned) {
				statement = session.Batcher.PrepareStatement(sql);
			} else {
				statement = session.Batcher.PrepareBatchStatement(sql);
			}

			try {

				// now write the values of fields onto the prepared statement

				int versionParam = Dehydrate(id, fields, includeProperty, statement, session);

				if ( IsVersioned ) {
					VersionType.NullSafeSet( statement, oldVersion, versionParam, session);
					Check( statement.ExecuteNonQuery(), id );
				} else {
					session.Batcher.AddToBatch(1);
				}
			} catch (Exception e) {
				throw e;
			} finally {
				if ( IsVersioned ) session.Batcher.CloseStatement(statement);
			}
		}

		public EntityPersister(PersistentClass model, ISessionFactoryImplementor factory) : base(model, factory) {

			// CLASS + TABLE

			System.Type mappedClass = model.PersistentClazz;
			this.factory = factory;
			Table table = model.RootTable;
			qualifiedTableName = table.GetQualifiedName( factory.DefaultSchema );
			tableNames = new string[] { qualifiedTableName };

			// DISCRIMINATOR

			object discriminatorValue;
			if ( model.IsPolymorphic ) {
				Value d = model.Discriminator;
				if (d==null) throw new MappingException("discriminator mapping required for polymorphic persistence");
				foreach( Column col in d.ColumnCollection) {
					discriminatorColumnName = col.Name; //only once
				}
				try {
					discriminatorType = (IDiscriminatorType) model.Discriminator.Type;
					if ( "null".Equals( model.DiscriminatorValue)) {
						discriminatorValue = null;
						discriminatorSQLString = "null";
					} else {
						discriminatorValue = discriminatorType.StringToObject( model.DiscriminatorValue );
						discriminatorSQLString = discriminatorType.ObjectToSQLString(discriminatorValue);
					}
				} catch (Exception e) {
					throw new MappingException("Could not format discriminator value to sql string", e);
				}
			} else {
				discriminatorColumnName = null;
				discriminatorValue = null;
				discriminatorType = null;
				discriminatorSQLString = null;
			}

			// PROPERTIES

			propertyColumnNames = new string[hydrateSpan][];
			propertyColumnAliases = new string[hydrateSpan][];
			propertyColumnSpans = new int[hydrateSpan];
			ArrayList thisClassProperties = new ArrayList();

			int i=0;
			bool foundColumn = false;
			foreach(Property prop in model.PropertyClosureCollection) {
				int span = prop.ColumnSpan;
				propertyColumnSpans[i] = span;
				thisClassProperties.Add(prop);

				string[] colNames = new string[span];
				string[] colAliases = new string[span];
				int j=0;
				foreach(Column col in prop.ColumnCollection) {
					colAliases[j] = col.Alias;
					colNames[j] = col.Name;
					j++;
					foundColumn=true;
				}
				propertyColumnNames[i] = colNames;
				propertyColumnAliases[i] = colAliases;

				InitPropertyPaths(prop, StringHelper.EmptyString);
				i++;
			}

			if ( model.HasIdentifierProperty && model.IdentifierProperty.IsComposite ) {
				InitPropertyPaths( model.IdentifierProperty, StringHelper.EmptyString );
			}

			hasColumns = foundColumn;

			ArrayList columns = new ArrayList();
			ArrayList types = new ArrayList();
			ArrayList propColumns = new ArrayList();
			ArrayList aliases = new ArrayList();
			ArrayList joinedFetchesList = new ArrayList();
			ArrayList definedBySubclass = new ArrayList();

			foreach(Property prop in model.SubclassPropertyClosureCollection) {
				definedBySubclass.Add( !thisClassProperties.Contains(prop) );
				string[] cols = new string[ prop.ColumnSpan ];
				types.Add( prop.Type );
				int l=0;
				foreach( Column col in prop.ColumnCollection ) {
					columns.Add( col.Name );
					aliases.Add( col.Alias );
					cols[l++] = col.Name;
				}
				propColumns.Add(cols);
				joinedFetchesList.Add( prop.Value.OuterJoinFetchSetting );
			}
			subclassColumnClosure = (string[]) columns.ToArray(typeof(string));
			subclassPropertyTypeClosure = (IType[]) types.ToArray(typeof(IType));
			subclassPropertyColumnNameClosure = (string[][]) propColumns.ToArray( typeof(string[]));
				subclassColumnAliasClosure = (string[]) aliases.ToArray(typeof(string));

			joinedFetch = new OuterJoinLoaderType[ joinedFetchesList.Count ];
			int m=0;
			foreach(OuterJoinLoaderType qq in joinedFetchesList) {
				joinedFetch[m++] = qq;
			}
			definedOnSubclass = new bool[definedBySubclass.Count];
			m=0;
			foreach( bool val in definedBySubclass) {
				definedOnSubclass[m++] = val;
			}

			deleteString = GenerateDeleteString();
			insertString = GenerateInsertString( false, PropertyInsertability );
			identityInsertString = IsIdentifierAssignedByInsert ?
				GenerateInsertString( true, PropertyInsertability ) :
				null;
			updateString = GenerateUpdateString( PropertyUpdateability );

			string lockString = GenerateLockString();
			lockers.Add( LockMode.Read, lockString );
			string lockExclusiveString = dialect.SupportsForUpdate ? lockString + " for update" : lockString;
			lockers.Add( LockMode.Upgrade, lockExclusiveString);
			string lockExclusiveNowaitString = dialect.SupportsForUpdateNoWait ? lockString + " for update nowait" : lockExclusiveString;
			lockers.Add( LockMode.UpgradeNoWait, lockExclusiveNowaitString);

			int subclassSpan = model.SubclassSpan + 1;
			subclassClosure = new System.Type[subclassSpan];
			subclassClosure[0] = mappedClass;
			if ( model.IsPolymorphic ) {
				if (discriminatorValue==null) {
					subclassesByDiscriminatorValue.Add( ObjectUtils.Null, mappedClass);
				} else {
					subclassesByDiscriminatorValue.Add( discriminatorValue, mappedClass);
				}

			}

			// SUBCLASSES
			if ( model.IsPolymorphic ) {
				int k=1;
				foreach(Subclass sc in model.SubclassCollection) {
					subclassClosure[k++] = sc.PersistentClazz;
					if ("null".Equals( sc.DiscriminatorValue ) ) {
						subclassesByDiscriminatorValue.Add ( ObjectUtils.Null, sc.PersistentClazz );
					} else {
						try {
							subclassesByDiscriminatorValue.Add (
								discriminatorType.StringToObject( sc.DiscriminatorValue ),
								sc.PersistentClazz);
						} catch (Exception e) {
							throw new MappingException("Error parsing discriminator value", e);
						}
					}
				}
			}
		}

		private void InitPropertyPaths(Property prop, string path) {
			path += prop.Name;
			if (prop.IsComposite ) {
				foreach(Property subProp in ((Component)prop.Value).PropertyCollection) {
					typesByPropertyPath.Add( path, prop.Type);
					InitPropertyPaths( subProp, path + "." );
				}
			}

			string[] names = new string[ prop.ColumnSpan ];
			int k=0;
			foreach( Column col in prop.ColumnCollection ) {
				names[k] = col.Name;
				k++;
			}

			IType type = prop.Type;
			typesByPropertyPath.Add(path, type);

			columnNamesByPropertyPath.Add(path, names);
		}

		public string[] TableNames {
			get { return tableNames; }
		}

		public override string FromTableFragment(string name) {
			return TableName + ' ' + name;
		}

		public override string QueryWhereFragment(string name, bool innerJoin, bool includeSubclasses) {
			if (innerJoin && IsInherited ) {
				InFragment frag = new InFragment()
					.SetColumn ( name, DiscriminatorColumnName );
				System.Type[] subclasses = SubclassClosure;
				for ( int i=0; i<subclasses.Length; i++) {
					frag.AddValue(
						((IQueryable) factory.GetPersister(subclasses[i])).DiscriminatorSQLString
						);
				}
				return " and " + frag.ToFragmentString();
			} else {
				return StringHelper.EmptyString;
			}
		}

		public override string[] ToColumns(string name, string property) {
			string[] idcols = base.ToColumns(name, property);
			if (idcols!=null) return idcols;

			string[] cols = GetPropertyColumnNames(property);

			if (cols==null) throw new QueryException("unresolved property: " + property);

			if (cols.Length==0) {
				// ie a nested collection or a one-to-one
				cols = IdentifierColumnNames;
			}

			return StringHelper.Prefix(cols, name + StringHelper.Dot);
		}

		public override string[] ToColumns(string name, int i) {
			return StringHelper.Prefix( subclassPropertyColumnNameClosure[i], name + StringHelper.Dot );
		}

		public override string PropertySelectFragment(string name, string suffix) {
			SelectFragment frag = new SelectFragment()
				.SetSuffix(suffix);
			if ( HasSubclasses ) frag.AddColumn( name, DiscriminatorColumnName );
			return frag.AddColumns(name, subclassColumnClosure, subclassColumnAliasClosure)
				.ToFragmentString();
		}

		public override string GetConcreteClassAlias(string alias) {
			return alias;
		}

		public override string FromJoinFragment(string alias, bool innerJoin, bool includeSubclasses) {
			return StringHelper.EmptyString;
		}

		public override string WhereJoinFragment(string alias, bool innerJoin, bool includeSublasses) {
			return StringHelper.EmptyString;
		}


	}
}
