using System;

namespace NHibernate 
{
	[Serializable]
	public class CallbackException : HibernateException 
	{
		public CallbackException() : base("An exception occured in a callback") {}

		public CallbackException(Exception root) : base("An exception occured in a callback", root) {}

		public CallbackException(string message) : base(message) {}

		public CallbackException(string message, Exception e) : base(message, e) {}
	}
}
