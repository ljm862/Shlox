using LoxInterpreter.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter.Exceptions
{
	public class RuntimeError : SystemException
	{
		public readonly Token Token;

		public RuntimeError(Token token, string message) : base(message)
		{
			this.Token = token;
		}

		public RuntimeError()
		{
		}

		public RuntimeError(string message) : base(message)
		{
		}

		public RuntimeError(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected RuntimeError(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
