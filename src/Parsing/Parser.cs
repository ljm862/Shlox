using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoxInterpreter.Exceptions;
using LoxInterpreter.Lexing;

namespace LoxInterpreter.Parsing
{
	public class Parser
	{

		private readonly List<Token> tokens;
		private int current = 0;

		public Parser(List<Token> tokens)
		{
			this.tokens = tokens;
		}

		public List<Stmt> Parse()
		{
			var statements = new List<Stmt>();
			while (!this.IsAtEnd())
			{
				statements.Add(this.Declaration());
			}
			return statements;
		}

		#region Recursive Descent Steps

		private Expr Expression()
		{
			return this.Assignment();
		}

		private Expr Assignment()
		{
			var expr = this.Equality();

			if (this.Match(TokenType.EQUAL))
			{
				var equals = this.Previous();
				var value = this.Assignment();

				if (expr is Expr.Variable variable)
				{
					var name = variable.name;
					return new Expr.Assign(name, value);
				}

				this.Error(equals, "Invalid assignment target.");
			}

			return expr;
		}

		private Stmt Declaration()
		{
			try
			{
				if (this.Match(TokenType.VAR)) return this.VarDeclaration();
				return this.Statement();
			}
			catch (ParseError)
			{
				this.Synchronise();
				return null;
			}
		}

		private Stmt Statement()
		{
			if (this.Match(TokenType.PRINT)) return this.PrintStatement();
			if (this.Match(TokenType.LEFT_BRACE)) return new Stmt.Block(this.Block());

			return this.ExpressionStatement();
		}

		private Stmt PrintStatement()
		{
			var value = this.Expression();
			this.Consume(TokenType.SEMICOLON, "Expect ';' after value.");
			return new Stmt.Print(value);
		}

		private Stmt VarDeclaration()
		{
			var name = this.Consume(TokenType.IDENTIFIER, "Expect variable name.");

			Expr initializer = null;
			if (this.Match(TokenType.EQUAL))
			{
				initializer = this.Expression();
			}

			this.Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
			return new Stmt.Var(name, initializer);
		}

		private Stmt ExpressionStatement()
		{
			var expr = this.Expression();
			this.Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
			return new Stmt.Expression(expr);
		}

		private List<Stmt> Block()
		{
			var statements = new List<Stmt>();

			while (!this.Check(TokenType.RIGHT_BRACE) && !this.IsAtEnd())
			{
				statements.Add(this.Declaration());
			}

			this.Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
			return statements;
		}

		private Expr Equality()
		{
			var expr = this.Comparison();

			while (this.Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
			{
				var oper = this.Previous();
				var right = this.Comparison();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Comparison()
		{
			var expr = this.Term();

			while (this.Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
			{
				var oper = this.Previous();
				var right = this.Term();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Term()
		{
			var expr = this.Factor();

			while (this.Match(TokenType.MINUS, TokenType.PLUS))
			{
				var oper = this.Previous();
				var right = this.Factor();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Factor()
		{
			var expr = this.Unary();

			while (this.Match(TokenType.ASTERISK, TokenType.SLASH))
			{
				var oper = this.Previous();
				var right = this.Unary();
				expr = new Expr.Binary(expr, oper, right);
			}

			return expr;
		}

		private Expr Unary()
		{
			if (this.Match(TokenType.BANG, TokenType.MINUS))
			{
				var oper = this.Previous();
				var right = this.Unary();
				return new Expr.Unary(oper, right);
			}

			return this.Primary();
		}

		private Expr Primary()
		{
			if (this.Match(TokenType.FALSE)) return new Expr.Literal(false);
			if (this.Match(TokenType.TRUE)) return new Expr.Literal(true);
			if (this.Match(TokenType.NIL)) return new Expr.Literal(null);

			if (this.Match(TokenType.NUMBER, TokenType.STRING))
			{
				return new Expr.Literal(this.Previous().Literal);
			}

			if (this.Match(TokenType.IDENTIFIER))
			{
				return new Expr.Variable(this.Previous());
			}

			if (this.Match(TokenType.LEFT_PAREN))
			{
				var expr = this.Expression();
				this.Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
				return new Expr.Grouping(expr);
			}

			throw (this.Error(this.Peek(), "Expect expression."));
		}

		#endregion

		#region Utilities

		private bool Match(params TokenType[] types)
		{
			foreach (var type in types)
			{
				if (this.Check(type))
				{
					this.Advance();
					return true;
				}
			}
			return false;
		}

		private bool Check(TokenType type)
		{
			if (this.IsAtEnd()) return false;
			return this.Peek().Type == type;
		}

		private Token Advance()
		{
			if (!this.IsAtEnd()) current += 1;
			return this.Previous();
		}

		private bool IsAtEnd()
		{
			return this.Peek().Type == TokenType.EOF;
		}

		private Token Peek()
		{
			return this.tokens[this.current];
		}

		private Token Previous()
		{
			return this.tokens[this.current - 1];
		}

		#region Error Handling

		private Token Consume(TokenType type, string message)
		{
			if (this.Check(type)) return this.Advance();

			throw this.Error(this.Peek(), message);
		}

		private ParseError Error(Token token, string message)
		{
			Lox.Error(token, message);
			return new ParseError(message);
		}

		private void Synchronise()
		{
			this.Advance();

			while (!this.IsAtEnd())
			{
				if (this.Previous().Type == TokenType.SEMICOLON) return;
				switch (this.Peek().Type)
				{
					case TokenType.CLASS:
					case TokenType.FUN:
					case TokenType.VAR:
					case TokenType.FOR:
					case TokenType.IF:
					case TokenType.WHILE:
					case TokenType.PRINT:
					case TokenType.RETURN:
						return;
				}
				this.Advance();
			}
		}
		#endregion
		#endregion
	}
}
