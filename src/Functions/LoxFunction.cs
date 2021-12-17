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
		public LoxFunction(Stmt.Function declaration, Environment closure)
		{
			this.declaration = declaration;
			this.closure = closure;
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
				return returnValue.value;
			}

			return null;
		}

		public override string ToString() => $"<fn {this.declaration.name.Lexeme}>";
	}

}
