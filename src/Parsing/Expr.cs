using System;
using System.Collections.Generic;
using LoxInterpreter.Lexing;

namespace LoxInterpreter
{
	public abstract class Expr
	{
		public interface IVisitor<T>
		{
			T VisitAssignExpr(Assign expr);
			T VisitBinaryExpr(Binary expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLiteralExpr(Literal expr);
			T VisitLogicalExpr(Logical expr);
			T VisitUnaryExpr(Unary expr);
			T VisitVariableExpr(Variable expr);
		}
		public class Assign : Expr
		{
			public Assign(Token name, Expr value)
			{
				this.name = name;
				this.value = value;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitAssignExpr(this);
			}

			public readonly Token name;
			public readonly Expr value;
		}
		public class Binary : Expr
		{
			public Binary(Expr left, Token oper, Expr right)
			{
				this.left = left;
				this.oper = oper;
				this.right = right;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitBinaryExpr(this);
			}

			public readonly Expr left;
			public readonly Token oper;
			public readonly Expr right;
		}
		public class Grouping : Expr
		{
			public Grouping(Expr expression)
			{
				this.expression = expression;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitGroupingExpr(this);
			}

			public readonly Expr expression;
		}
		public class Literal : Expr
		{
			public Literal(Object value)
			{
				this.value = value;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitLiteralExpr(this);
			}

			public readonly Object value;
		}
		public class Logical : Expr
		{
			public Logical(Expr left, Token oper, Expr right)
			{
				this.left = left;
				this.oper = oper;
				this.right = right;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitLogicalExpr(this);
			}

			public readonly Expr left;
			public readonly Token oper;
			public readonly Expr right;
		}
		public class Unary : Expr
		{
			public Unary(Token oper, Expr right)
			{
				this.oper = oper;
				this.right = right;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitUnaryExpr(this);
			}

			public readonly Token oper;
			public readonly Expr right;
		}
		public class Variable : Expr
		{
			public Variable(Token name)
			{
				this.name = name;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitVariableExpr(this);
			}

			public readonly Token name;
		}

		public abstract T Accept<T>(IVisitor<T> visitor);
	}
}
