using LoxInterpreter.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter.Functions
{
	public class LoxClass : ILoxCallable
	{
		public readonly string Name;
		private readonly Dictionary<string, LoxFunction> methods;

		public LoxClass(string name, Dictionary<string, LoxFunction> methods)
		{
			this.Name = name;
			this.methods = methods;
		}

		public int Arity()
		{
			return 0;
		}

		public object Call(Interpreter interpreter, List<object> arguments)
		{
			var instance = new LoxInstance(this);
			return instance;
		}

		public LoxFunction FindMethod(string name)
		{
			return this.methods.TryGetValue(name, out var value) ? value : null;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
