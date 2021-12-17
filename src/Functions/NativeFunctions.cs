using LoxInterpreter.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter
{
	public class NativeFunctions
	{
		public class ClockFunction : ILoxCallable
		{
			public int Arity() => 0;

			public object Call(Interpreter interpreter, List<object> arguments) => (double)System.Environment.TickCount / 1000.0;

			public override string ToString() => "<native fn>";
		}
	}
}
