using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter.Exceptions
{
	public class Return : SystemException
	{
		public readonly object value;

		public Return(object value)
		{
			this.value = value;
		}
	}
}
