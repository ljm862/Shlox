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
			var expr = this.Or();

			if (this.Match(TokenType.EQUAL))
			{
				var equals = this.Previous();
				var value = this.Assignment();

				if (expr is Expr.Variable variable)
				{
					var name = variable.name;
					return new Expr.Assign(name, value);
				}
				else if (expr is Expr.Get get)
				{
					return new Expr.Set(get.obj, get.name, value);
				}

				this.Error(equals, "Invalid assignment target.");
			}

			return expr;
		}

		private Expr Or()
		{
			var expr = this.And();

			while (this.Match(TokenType.OR))
			{
				var oper = this.Previous();
				var right = this.And();
				expr = new Expr.Logical(expr, oper, right);
			}
			return expr;
		}

		private Expr And()
		{
			var expr = this.Equality();

			while (this.Match(TokenType.AND))
			{
				var oper = this.Previous();
				var right = this.Equality();
				expr = new Expr.Logical(expr, oper, right);
			}
			return expr;
		}

		private Stmt Declaration()
		{
			try
			{
				if (this.Match(TokenType.CLASS)) return this.ClassDeclaration();
				if (this.Match(TokenType.FUN)) return this.Function("function");
				if (this.Match(TokenType.VAR)) return this.VarDeclaration();
				return this.Statement();
			}
			catch (ParseError)
			{
				this.Synchronise();
				return null;
			}
		}

		private Stmt ClassDeclaration()
		{
			var name = this.Consume(TokenType.IDENTIFIER, "Expect class name.");
			this.Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

			var methods = new List<Stmt.Function>();
			while (!this.Check(TokenType.RIGHT_BRACE) && !this.IsAtEnd())
			{
				methods.Add(this.Function("method"));
			}

			this.Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");
			return new Stmt.Class(name, methods);
		}

		private Stmt Statement()
		{
			if (this.Match(TokenType.FOR)) return this.ForStatement();
			if (this.Match(TokenType.IF)) return this.IfStatement();
			if (this.Match(TokenType.PRINT)) return this.PrintStatement();
			if (this.Match(TokenType.RETURN)) return this.ReturnStatement();
			if (this.Match(TokenType.WHILE)) return this.WhileStatement();
			if (this.Match(TokenType.LEFT_BRACE)) return new Stmt.Block(this.Block());

			return this.ExpressionStatement();
		}

		private Stmt ForStatement()
		{
			this.Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
			Stmt initializer;
			if (this.Match(TokenType.SEMICOLON))
			{
				initializer = null;
			}
			else if (this.Match(TokenType.VAR))
			{
				initializer = this.VarDeclaration();
			}
			else
			{
				initializer = this.ExpressionStatement();
			}

			Expr condition = null;
			if (!this.Check(TokenType.SEMICOLON))
			{
				condition = this.Expression();
			}
			this.Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

			Expr increment = null;
			if (!this.Check(TokenType.RIGHT_PAREN))
			{
				increment = this.Expression();
			}
			this.Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

			var body = this.Statement();

			if (increment != null)
			{
				body = new Stmt.Block(new List<Stmt>() { body, new Stmt.Expression(increment) });
			}

			if (condition == null) condition = new Expr.Literal(true);
			body = new Stmt.While(condition, body);

			if (initializer != null)
			{
				body = new Stmt.Block(new List<Stmt>() { initializer, body });
			}

			return body;
		}

		private Stmt IfStatement()
		{
			this.Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
			var condition = this.Expression();
			this.Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");

			var thenBranch = this.Statement();
			Stmt elseBranch = null;
			if (this.Match(TokenType.ELSE))
			{
				elseBranch = this.Statement();
			}

			return new Stmt.If(condition, thenBranch, elseBranch);
		}

		private Stmt PrintStatement()
		{
			var value = this.Expression();
			this.Consume(TokenType.SEMICOLON, "Expect ';' after value.");
			return new Stmt.Print(value);
		}

		private Stmt ReturnStatement()
		{
			var keyword = this.Previous();
			Expr value = null;
			if (!this.Check(TokenType.SEMICOLON))
			{
				value = this.Expression();
			}
			this.Consume(TokenType.SEMICOLON, "Expect ';' after return value.");
			return new Stmt.Return(keyword, value);
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

		private Stmt WhileStatement()
		{
			this.Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
			var condition = this.Expression();
			this.Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
			var body = this.Statement();

			return new Stmt.While(condition, body);
		}

		private Stmt ExpressionStatement()
		{
			var expr = this.Expression();
			if (this.Match(TokenType.TERNARY))
			{
				return this.TernaryStatement(expr);
			}
			this.Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
			return new Stmt.Expression(expr);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="kind">Typically will be function or method, so we know what to write as an error</param>
		/// <returns></returns>
		private Stmt.Function Function(string kind)
		{
			var name = this.Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
			this.Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
			var parameters = new List<Token>();

			if (!this.Check(TokenType.RIGHT_PAREN))
			{
				do
				{
					if (parameters.Count >= 255)
					{
						this.Error(this.Peek(), "Can't have more than 255 parameters.");
					}
					parameters.Add(this.Consume(TokenType.IDENTIFIER, "Expect parameter name."));
				} while (this.Match(TokenType.COMMA));
			}
			this.Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

			this.Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
			var body = this.Block();
			return new Stmt.Function(name, parameters, body);
		}

		private Stmt TernaryStatement(Expr conditional)
		{
			var thenBranch = this.Statement();
			this.Consume(TokenType.COLON, "Expect ':' after then statement.");
			var elseBranch = this.Statement();
			this.Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
			return new Stmt.If(conditional, thenBranch, elseBranch);
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

			return this.Call();
		}

		private Expr FinishCall(Expr callee)
		{
			var args = new List<Expr>();
			if (!this.Check(TokenType.RIGHT_PAREN))
			{
				do
				{
					if (args.Count >= 255)
					{
						this.Error(Peek(), "Can't have more than 255 arguments.");
					}
					args.Add(this.Expression());
				} while (this.Match(TokenType.COMMA));
			}

			var paren = this.Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

			return new Expr.Call(callee, paren, args);
		}

		private Expr Call()
		{
			var expr = this.Primary();

			while (true)
			{
				if (this.Match(TokenType.LEFT_PAREN))
				{
					expr = this.FinishCall(expr);
				}
				else if (this.Match(TokenType.DOT))
				{
					var name = this.Consume(TokenType.IDENTIFIER, "Expect property name affter '.' .");
					expr = new Expr.Get(expr, name);
				}
				else
				{
					break;
				}
			}
			return expr;
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
