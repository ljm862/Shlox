using LoxInterpreter.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter.Parsing
{
	public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
	{
		private readonly Interpreter interpreter;
		private readonly Stack<Dictionary<string, bool>> scopes = new();
		private FunctionType currentFunction = FunctionType.NONE;

		private enum FunctionType
		{
			NONE,
			FUNCTION
		}

		public Resolver(Interpreter interpreter)
		{
			this.interpreter = interpreter;
		}

		public void BeginScope()
		{
			this.scopes.Push(new Dictionary<string, bool>());
		}

		public void EndScope()
		{
			this.scopes.Pop();
		}

		private void Declare(Token name)
		{
			if (this.scopes.Count < 1) return;
			var scope = scopes.Peek();

			if (scope.ContainsKey(name.Lexeme))
			{
				Lox.Error(name, "Already a variable with this name in this scope.");
			}

			scope.Add(name.Lexeme, false);
		}

		private void Define(Token name)
		{
			if (this.scopes.Count < 1) return;
			this.scopes.Peek().Add(name.Lexeme, true);
		}

		private void ResolveLocal(Expr expr, Token name)
		{
			for (int i = this.scopes.Count - 1; i >= 0; i--)
			{
				if (this.scopes.ElementAt(i).ContainsKey(name.Lexeme))
				{
					this.interpreter.Resolve(expr, this.scopes.Count() - 1 - i);
					return;
				}
			}
		}

		public void Resolve(List<Stmt> statements)
		{
			foreach (var statement in statements)
			{
				this.Resolve(statement);
			}
		}

		private void Resolve(Stmt stmt)
		{
			stmt.Accept(this);
		}

		private void Resolve(Expr expr)
		{
			expr.Accept(this);
		}

		private void ResolveFunction(Stmt.Function function, FunctionType type)
		{
			var enclosingFunction = this.currentFunction;
			this.currentFunction = type;

			this.BeginScope();
			foreach (var param in function.parameters)
			{
				this.Declare(param);
				this.Define(param);
			}
			this.Resolve(function.body);
			this.EndScope();
			this.currentFunction = enclosingFunction;
		}

		#region Visitor Pattern Interface Implementations

		public object VisitAssignExpr(Expr.Assign expr)
		{
			this.Resolve(expr.value);
			this.ResolveLocal(expr, expr.name);
			return null;
		}

		public object VisitBinaryExpr(Expr.Binary expr)
		{
			this.Resolve(expr.left);
			this.Resolve(expr.right);
			return null;
		}

		public object VisitBlockStmt(Stmt.Block stmt)
		{
			this.BeginScope();
			this.Resolve(stmt.statements);
			this.EndScope();
			return null;
		}

		public object VisitCallExpr(Expr.Call expr)
		{
			this.Resolve(expr.callee);
			foreach (var argument in expr.arguments)
			{
				this.Resolve(argument);
			}
			return null;
		}

		public object VisitExpressionStmt(Stmt.Expression stmt)
		{
			this.Resolve(stmt.expression);
			return null;
		}

		public object VisitFunctionStmt(Stmt.Function stmt)
		{
			this.Declare(stmt.name);
			this.Define(stmt.name);

			this.ResolveFunction(stmt, FunctionType.FUNCTION);
			return null;
		}

		public object VisitGroupingExpr(Expr.Grouping expr)
		{
			this.Resolve(expr.expression);
			return null;
		}

		public object VisitIfStmt(Stmt.If stmt)
		{
			this.Resolve(stmt.condition);
			this.Resolve(stmt.thenBranch);
			if (stmt.elseBranch != null) this.Resolve(stmt.elseBranch);
			return null;
		}

		public object VisitLiteralExpr(Expr.Literal expr)
		{
			// Doesn't mention any variables and doesn't contain expressions.
			return null;
		}

		public object VisitLogicalExpr(Expr.Logical expr)
		{
			this.Resolve(expr.left);
			this.Resolve(expr.right);
			return null;
		}

		public object VisitPrintStmt(Stmt.Print stmt)
		{
			this.Resolve(stmt.expression);
			return null;
		}

		public object VisitReturnStmt(Stmt.Return stmt)
		{
			if (this.currentFunction == FunctionType.NONE)
			{
				Lox.Error(stmt.keyword, "Can't return from top-level code.");
			}
			if (stmt.value != null) this.Resolve(stmt.value);
			return null;
		}

		public object VisitUnaryExpr(Expr.Unary expr)
		{
			this.Resolve(expr.right);
			return null;
		}

		public object VisitVariableExpr(Expr.Variable expr)
		{
			if (this.scopes.Count > 0 && this.scopes.Peek()[expr.name.Lexeme] == false)
			{
				Lox.Error(expr.name, "Can't read local variable in its own initializer.");
			}

			this.ResolveLocal(expr, expr.name);
			return null;
		}

		public object VisitVarStmt(Stmt.Var stmt)
		{
			this.Declare(stmt.name);
			if (stmt.initializer != null)
			{
				this.Resolve(stmt.initializer);
			}
			this.Define(stmt.name);
			return null;
		}

		public object VisitWhileStmt(Stmt.While stmt)
		{
			this.Resolve(stmt.condition);
			this.Resolve(stmt.body);
			return null;
		}

		#endregion

	}
}
