using System;
using System.Collections;

using Iesi.Collections;

namespace NHibernate.DomainModel
{
	[Serializable]
	public class Baz : INamed, IComparable
	{
		#region Fields

		private NestingComponent _collectionComponent;
		private String _code;
		private FooComponent[] _components;
		private DateTime[] _timeArray;
		private string[] _stringArray;
		private int[] _intArray;
		private FooProxy[] _fooArray;
		private Int32 _count;
		private String _name;
		private Foo _foo;
		private IList _stringList;
		private IList _fees;
		private IList _customs;
		private IList _topComponents;
		private IDictionary _fooToGlarch;
		private IDictionary _fooComponentToFoo;
		private IDictionary _glarchToFoo;
		private IDictionary _stringDateMap;
		private IDictionary _topGlarchez;
		private IDictionary _cachedMap;
		private IDictionary _stringGlarchMap;
		private IDictionary _anyToAny;
		private IList _manyToAny;
		private ISet _fooSet;
		private ISet _stringSet;
		private ISet _topFoos;
		private ISet _cascadingBars;
		private ISet _cached;
		private ISet _sortablez;
		private IList _bag;
		private IList _fooBag;
		private IList _idFooBag;
		private IList _byteBag;
		private IList _bazez;
		private IList _parts;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor for class Baz
		/// </summary>
		public Baz()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the _collectionComponent
		/// </summary> 
		public NestingComponent CollectionComponent
		{
			get { return _collectionComponent; }
			set { _collectionComponent = value; }
		}

		/// <summary>
		/// Get/set for Code
		/// </summary>
		public String Code
		{
			get { return this._code; }
			set { this._code = value; }
		}

		/// <summary>
		/// Get/set for count
		/// </summary>
		public Int32 Count
		{
			get { return this._count; }
			set { this._count = value; }
		}

		/// <summary>
		/// Get/set for name
		/// </summary>
		public String Name
		{
			get { return this._name; }
			set { this._name = value; }
		}

		/// <summary>
		/// Get/set for Foo
		/// </summary>
		public Foo Foo
		{
			get { return this._foo; }
			set { this._foo = value; }
		}

		/// <summary>
		/// Get/set for stringList
		/// </summary>
		public IList StringList
		{
			get { return this._stringList; }
			set { this._stringList = value; }
		}

		/// <summary>
		/// Get/set for fees
		/// </summary>
		public IList Fees
		{
			get { return _fees; }
			set { _fees = value; }
		}

		/// <summary>
		/// Get/set for customs
		/// </summary>
		public IList Customs
		{
			get { return this._customs; }
			set { this._customs = value; }
		}

		/// <summary>
		/// Get/set for topComponents
		/// </summary>
		public IList TopComponents
		{
			get { return this._topComponents; }
			set { this._topComponents = value; }
		}

		/// <summary>
		/// Get/set for fooToGlarch
		/// </summary>
		public IDictionary FooToGlarch
		{
			get { return this._fooToGlarch; }
			set { this._fooToGlarch = value; }
		}

		/// <summary>
		/// Get/set for fooComponentToFoo
		/// </summary>
		public IDictionary FooComponentToFoo
		{
			get { return this._fooComponentToFoo; }
			set { this._fooComponentToFoo = value; }
		}

		/// <summary>
		/// Get/set for glarchToFoo
		/// </summary>
		public IDictionary GlarchToFoo
		{
			get { return this._glarchToFoo; }
			set { this._glarchToFoo = value; }
		}

		/// <summary>
		/// Get/set for stringDateMap
		/// </summary>
		public IDictionary StringDateMap
		{
			get { return this._stringDateMap; }
			set { this._stringDateMap = value; }
		}

		/// <summary>
		/// Get/set for topGlarchez
		/// </summary>
		public IDictionary TopGlarchez
		{
			get { return this._topGlarchez; }
			set { this._topGlarchez = value; }
		}

		/// <summary>
		/// Get/set for cachedMap
		/// </summary>
		public IDictionary CachedMap
		{
			get { return this._cachedMap; }
			set { this._cachedMap = value; }
		}

		/// <summary>
		/// Get/set for stringGlarchMap
		/// </summary>
		public IDictionary StringGlarchMap
		{
			get { return this._stringGlarchMap; }
			set { this._stringGlarchMap = value; }
		}

		/// <summary>
		/// Get/set for anyToAny
		/// </summary>
		public IDictionary AnyToAny
		{
			get { return this._anyToAny; }
			set { this._anyToAny = value; }
		}

		/// <summary>
		/// Get/set for manyToAny
		/// </summary>
		public IList ManyToAny
		{
			get { return this._manyToAny; }
			set { this._manyToAny = value; }
		}

		/// <summary>
		/// Gets or sets the intArray
		/// </summary> 
		public int[] IntArray
		{
			get { return _intArray; }
			set { _intArray = value; }
		}

		/// <summary>
		/// Gets or sets the _components
		/// </summary> 
		public FooComponent[] Components
		{
			get { return _components; }
			set { _components = value; }
		}

		/// <summary>
		/// Gets or sets the timeArray
		/// </summary> 
		public DateTime[] TimeArray
		{
			get { return _timeArray; }
			set { _timeArray = value; }
		}

		/// <summary>
		/// Gets or sets the stringArray
		/// </summary> 
		public string[] StringArray
		{
			get { return _stringArray; }
			set { _stringArray = value; }
		}


		/// <summary>
		/// Gets or sets the fooArray
		/// </summary> 
		public FooProxy[] FooArray
		{
			get { return _fooArray; }
			set { _fooArray = value; }
		}


		/// <summary>
		/// Get/set for fooSet
		/// </summary>
		public ISet FooSet
		{
			get { return this._fooSet; }
			set { this._fooSet = value; }
		}

		/// <summary>
		/// Get/set for stringSet
		/// </summary>
		public ISet StringSet
		{
			get { return this._stringSet; }
			set { this._stringSet = value; }
		}

		/// <summary>
		/// Get/set for topFoos
		/// </summary>
		public ISet TopFoos
		{
			get { return this._topFoos; }
			set { this._topFoos = value; }
		}

		/// <summary>
		/// Get/set for cascadingBars
		/// </summary>
		public ISet CascadingBars
		{
			get { return this._cascadingBars; }
			set { this._cascadingBars = value; }
		}

		/// <summary>
		/// Get/set for cached
		/// </summary>
		public ISet Cached
		{
			get { return this._cached; }
			set { this._cached = value; }
		}

		/// <summary>
		/// Get/set for sortablez
		/// </summary>
		public ISet Sortablez
		{
			get { return this._sortablez; }
			set { this._sortablez = value; }
		}

		/// <summary>
		/// Get/set for bag
		/// </summary>
		public IList Bag
		{
			get { return this._bag; }
			set { this._bag = value; }
		}

		/// <summary>
		/// Get/set for fooBag
		/// </summary>
		public IList FooBag
		{
			get { return this._fooBag; }
			set { this._fooBag = value; }
		}

		/// <summary>
		/// Get/set for bazez
		/// </summary>
		public IList Bazez
		{
			get { return this._bazez; }
			set { this._bazez = value; }
		}

		/// <summary>
		/// Get/set for idFooBag
		/// </summary>
		public IList IdFooBag
		{
			get { return this._idFooBag; }
			set { this._idFooBag = value; }
		}

		/// <summary>
		/// Get/set for byteBag
		/// </summary>
		public IList ByteBag
		{
			get { return this._byteBag; }
			set { this._byteBag = value; }
		}

		/// <summary>
		/// Get/set for parts
		/// </summary>
		public IList Parts
		{
			get { return this._parts; }
			set { this._parts = value; }
		}

		#endregion

		public void SetDefaults()
		{
			DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

			StringSet = new HashedSet();
			StringSet.Add("foo");
			StringSet.Add("bar");
			StringSet.Add("baz");

			StringDateMap = new SortedList();
			StringDateMap.Add("now", DateTime.Now);
			StringDateMap.Add("never", null);
			// according to SQL Server the big bag happened in 1753 ;)
			StringDateMap.Add("big bang", new DateTime(1753, 01, 01));
			//StringDateMap.Add( "millenium", new DateTime( 2000, 01, 01 ) );
			ArrayList list = new ArrayList();
			list.AddRange(StringSet);
			StringList = list;
			IntArray = new int[] {1, 3, 3, 7};
			FooArray = new Foo[0];
			StringArray = (String[]) list.ToArray(typeof(string));
			Customs = new ArrayList();
			Customs.Add(new String[] {"foo", "bar"});
			Customs.Add(new String[] {"A", "B"});
			Customs.Add(new String[] {"1", "2"});

			FooSet = new HashedSet();
			Components = new FooComponent[]
				{
					new FooComponent("foo", 42, null, null),
					new FooComponent("bar", 88, null, new FooComponent("sub", 69, null, null))
				};
			TimeArray = new DateTime[]
				{
					new DateTime(),
					new DateTime(),
					new DateTime(), // H2.1 has null here, but it's illegal on .NET
					new DateTime(0)
				};

			Count = 667;
			Name = "Bazza";
			TopComponents = new ArrayList();
			TopComponents.Add(new FooComponent("foo", 11, new DateTime[] {today, new DateTime(2123, 1, 1)}, null));
			TopComponents.Add(
				new FooComponent("bar", 22, new DateTime[] {new DateTime(2007, 2, 3), new DateTime(1945, 6, 1)}, null));
			TopComponents.Add(null);
			Bag = new ArrayList();
			Bag.Add("duplicate");
			Bag.Add("duplicate");
			Bag.Add("duplicate");
			Bag.Add("unique");

			Cached = new ListSet();

			CompositeElement ce = new CompositeElement();
			ce.Foo = "foo";
			ce.Bar = "bar";
			CompositeElement ce2 = new CompositeElement();
			ce2.Foo = "fooxxx";
			ce2.Bar = "barxxx";
			Cached.Add(ce);
			Cached.Add(ce2);
			CachedMap = new SortedList();
			CachedMap.Add(this, ce);
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			return ((Baz) obj).Code.CompareTo(Code);
		}

		#endregion
	}
}