using LoxInterpreter.Exceptions;
using LoxInterpreter.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter.Functions
{
	public class LoxFunction : ILoxCallable
	{
		private readonly Stmt.Function declaration;
		private readonly Environment closure;
		private readonly bool isInitializer;
		public LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
		{
			this.declaration = declaration;
			this.closure = closure;
			this.isInitializer = isInitializer;
		}

		public LoxFunction Bind(LoxInstance instance)
		{
			var environment = new Environment(this.closure);
			environment.Define("this", instance);
			return new LoxFunction(this.declaration, environment, this.isInitializer);
		}

		public int Arity() => this.declaration.parameters.Count;

		public object Call(Interpreter interpreter, List<object> arguments)
		{
			var environment = new Environment(this.closure);
			for (int i = 0; i < this.declaration.parameters.Count; i++)
			{
				environment.Define(this.declaration.parameters[i].Lexeme, arguments[i]);
			}

			try
			{
				interpreter.ExecuteBlock(declaration.body, environment);
			}
			catch (Return returnValue)
			{
				if (this.isInitializer) return closure.GetAt(0, "this");
				return returnValue.value;
			}

			if (this.isInitializer) return this.closure.GetAt(0, "this");

			return null;
		}

		public override string ToString() => $"<fn {this.declaration.name.Lexeme}>";
	}

}
