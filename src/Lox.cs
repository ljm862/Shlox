using LoxInterpreter.Lexing;
using LoxInterpreter.Parsing;
using LoxInterpreter.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace LoxInterpreter
{
	class Lox
	{
		private static readonly Interpreter interpreter = new();
		public static bool hadError = false;
		public static bool hadRuntimeError = false;
		static void Main(string[] args)
		{

			if (args.Length > 1)
			{
				Console.WriteLine("Usage: jlox [script]");
				Environment.Exit(64);
			}
			else if (args.Length == 1)
			{
				RunFile(args[0]);
			}
			else
			{
				RunPrompt();
			}

		}

		private static void RunFile(string path)
		{
			var bytes = File.ReadAllBytes(path);
			Run(System.Text.Encoding.Default.GetString(bytes));

			if (hadError) Environment.Exit(65);
			if (hadRuntimeError) Environment.Exit(70);
		}

		private static void RunPrompt()
		{
			var inputStream = Console.OpenStandardInput();
			var reader = new StreamReader(inputStream);

			while (true)
			{
				Console.Write("> ");
				var line = reader.ReadLine();
				if (line == null) break;
				Run(line);
				hadError = false;
			}
		}

		private static void Run(string sourceCode)
		{
			var scanner = new Scanner(sourceCode);
			var tokens = scanner.ScanTokens();
			var parser = new Parser(tokens);
			var expression = parser.Parse();

			if (hadError) return;

			interpreter.Interpret(expression);
		}

		public static void Error(int line, string message)
		{
			// Can aggregate these and output them all in a nice format
			// all at once before running instead of as they're found.
			Report(line, "", message);
		}

		public static void Error(Token token, string message)
		{
			if (token.Type == TokenType.EOF)
			{
				Report(token.Line, " at end", message);
			}
			else
			{
				Report(token.Line, $" at '{token.Lexeme}'", message);
			}
		}

		public static void RuntimeError(RuntimeError error)
		{
			Console.Error.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
			hadRuntimeError = true;
		}

		private static void Report(int line, string where, string message)
		{
			Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
			hadError = true;
		}
	}
}
