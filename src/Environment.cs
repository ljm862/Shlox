﻿using LoxInterpreter.Exceptions;
using LoxInterpreter.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter
{
	public class Environment
	{
		public readonly Environment Enclosing;
		private readonly Dictionary<string, object> values = new();

		public Environment()
		{
			Enclosing = null;
		}

		public Environment(Environment enclosing)
		{
			this.Enclosing = enclosing;
		}

		public object Get(Token name)
		{
			if (this.values.ContainsKey(name.Lexeme))
			{
				return this.values[name.Lexeme];
			}

			if (this.Enclosing != null) return this.Enclosing.Get(name);

			throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
		}

		public void Assign(Token name, object value)
		{
			if (this.values.ContainsKey(name.Lexeme))
			{
				this.Define(name.Lexeme, value);
				return;
			}

			if (this.Enclosing != null)
			{
				this.Enclosing.Assign(name, value);
				return;
			}

			throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
		}

		public void Define(string name, object value)
		{
			// This enables overwriting of the var's value.
			this.values[name] = value;
		}


	}
}