using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoxInterpreter.Parsing;

namespace LoxInterpreter
{
	public interface ILoxCallable
	{
		public int Arity();
		public object Call(Interpreter interpreter, List<object> arguments);

	}
}
