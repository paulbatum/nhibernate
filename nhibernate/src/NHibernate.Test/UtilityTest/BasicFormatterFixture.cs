using System;
using NHibernate.AdoNet.Util;
using NUnit.Framework;

namespace NHibernate.Test.UtilityTest
{
	[TestFixture]
	public class BasicFormatterFixture
	{
		[Test]
		public void StringWithNestedDelimiters()
		{
			Exception exception = null;
			string formattedSql = null;

			try
			{
				string sql = @"UPDATE Table SET Column = @p0;@p0 = '(')'";
				IFormatter formatter = new BasicFormatter();
				formattedSql = formatter.Format(sql);
			}			
			catch(Exception ex)
			{
				exception = ex;
			}

			Assert.IsNull(exception);
			Assert.IsNotNullOrEmpty(formattedSql);
		}
	}
}