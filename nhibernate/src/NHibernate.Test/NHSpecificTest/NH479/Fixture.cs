using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH479
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		public override string BugNumber
		{
			get { return "NH479"; }
		}

		[Test]
		public void SaveOrUpdateCopyTest()
		{
			Main main = new Main();
			Aggregate aggregate = new Aggregate();

			main.Aggregate = aggregate;
			aggregate.Main = main;

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				s.Save(main);
				t.Commit();
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					s.SaveOrUpdateCopy(main);
					s.SaveOrUpdateCopy(aggregate);
					t.Commit();
				}

				using (ITransaction t = s.BeginTransaction())
				{
					s.Delete("from Aggregate");
					s.Delete("from Main");
					t.Commit();
				}
			}
		}
	}
}