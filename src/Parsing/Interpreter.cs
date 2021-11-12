using LoxInterpreter.Exceptions;
using LoxInterpreter.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxInterpreter.Parsing
{
	public class Interpreter : Expr.IVisitor<object>
	{

		public void Interpret(Expr expression)
		{
			try
			{
				var value = this.Evaluate(expression);
				Console.WriteLine(this.Stringify(value));
			}
			catch (RuntimeError e)
			{
				Lox.RuntimeError(e);
			}
		}

		#region Interface
		public object VisitBinaryExpr(Expr.Binary expr)
		{
			var left = this.Evaluate(expr.left);
			var right = this.Evaluate(expr.right);

			switch (expr.oper.Type)
			{
				case TokenType.GREATER:
					this.CheckNumberOperands(expr.oper, left, right);
					return (double)left > (double)right;
				case TokenType.GREATER_EQUAL:
					this.CheckNumberOperands(expr.oper, left, right);
					return (double)left >= (double)right;
				case TokenType.LESS:
					this.CheckNumberOperands(expr.oper, left, right);
					return (double)left < (double)right;
				case TokenType.LESS_EQUAL:
					this.CheckNumberOperands(expr.oper, left, right);
					return (double)left <= (double)right;

				case TokenType.EQUAL_EQUAL:
					return this.IsEqual(left, right);
				case TokenType.BANG_EQUAL:
					return !this.IsEqual(left, right);

				case TokenType.MINUS:
					this.CheckNumberOperands(expr.oper, left, right);
					return (double)left - (double)right;
				case TokenType.SLASH:
					this.CheckNumberOperands(expr.oper, left, right);
					if ((double)right == 0) throw new RuntimeError(expr.oper, "Cannot divide by zero");
					return (double)left / (double)right;
				case TokenType.ASTERISK:
					this.CheckNumberOperands(expr.oper, left, right);
					return (double)left * (double)right;

				case TokenType.PLUS:
					if (left is double lhsD && right is double rhsD)
					{
						return lhsD + rhsD;
					}
					if (left is string lhsS && right is string rhsS)
					{
						return lhsS + rhsS;
					}
					throw new RuntimeError(expr.oper, "Operands must be two numbers OR two strings.");
			}
			return null;
		}

		public object VisitGroupingExpr(Expr.Grouping expr)
		{
			return this.Evaluate(expr.expression);
		}

		public object VisitLiteralExpr(Expr.Literal expr)
		{
			return expr.value;
		}

		public object VisitUnaryExpr(Expr.Unary expr)
		{
			//Evaluate the expression value first
			var right = this.Evaluate(expr.right);

			switch (expr.oper.Type)
			{
				case TokenType.BANG:
					return !this.IsTruthy(right);
				case TokenType.MINUS:
					this.CheckNumberOperand(expr.oper, right);
					return -(double)right;
			}
			return null;
		}
		#endregion

		/// <summary>
		/// Recursively send the value back through the visitor
		/// </summary>
		/// <param name="expr"></param>
		/// <returns></returns>
		private object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}

		private bool IsTruthy(object obj)
		{
			if (obj == null) return false;
			if (obj is bool boolObj) return boolObj;
			return true;
		}

		private bool IsEqual(object a, object b)
		{
			if (a == null && b == null) return true;
			if (a == null) return false;
			return a.Equals(b);
		}

		private string Stringify(object obj)
		{
			if (obj == null) return "nil";
			var text = obj.ToString();
			if (obj is double)
			{
				if (text.EndsWith(".0"))
				{
					text = text[0..^2];
				}
			}
			return text;
		}

		private void CheckNumberOperand(Token oper, object operand)
		{
			if (operand is double) return;
			throw new RuntimeError(oper, "Operand must be a number.");
		}

		private void CheckNumberOperands(Token oper, object left, object right)
		{
			if (left is double && right is double) return;
			throw new RuntimeError(oper, "Operands must be numbers.");
		}
	}
}
