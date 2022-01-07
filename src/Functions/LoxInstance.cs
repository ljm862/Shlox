using LoxInterpreter.Exceptions;
using LoxInterpreter.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter.Functions
{
	public class LoxInstance
	{
		private LoxClass loxClass;
		private readonly Dictionary<string, object> fields = new();

		public LoxInstance(LoxClass loxClass)
		{
			this.loxClass = loxClass;
		}

		public object Get(Token name)
		{
			if (this.fields.ContainsKey(name.Lexeme))
			{
				return this.fields[name.Lexeme];
			}

			var method = loxClass.FindMethod(name.Lexeme);
			if (method != null) return method;

			throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
		}

		public void Set(Token name, object value)
		{
			this.fields.Add(name.Lexeme, value);
		}

		public override string ToString()
		{
			return $"{this.loxClass.Name} instance";
		}
	}
}
