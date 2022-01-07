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
		public readonly LoxClass superclass;
		private readonly Dictionary<string, LoxFunction> methods;

		public LoxClass(string name, LoxClass superclass, Dictionary<string, LoxFunction> methods)
		{
			this.Name = name;
			this.superclass = superclass;
			this.methods = methods;
		}

		public int Arity()
		{
			var initializer = this.FindMethod("init");
			if (initializer == null) return 0;
			return initializer.Arity();
		}

		public object Call(Interpreter interpreter, List<object> arguments)
		{
			var instance = new LoxInstance(this);
			var initializer = this.FindMethod("init");
			if (initializer != null) initializer.Bind(instance).Call(interpreter, arguments);
			return instance;
		}

		public LoxFunction FindMethod(string name)
		{

			if (this.methods.ContainsKey(name))
			{
				return this.methods[name];
			}

			if (this.superclass != null)
			{
				return superclass.FindMethod(name);
			}

			return null;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
