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
			T VisitCallExpr(Call expr);
			T VisitGetExpr(Get expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLiteralExpr(Literal expr);
			T VisitLogicalExpr(Logical expr);
			T VisitSetExpr(Set expr);
			T VisitSuperExpr(Super expr);
			T VisitThisExpr(This expr);
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
		public class Call : Expr
		{
			public Call(Expr callee, Token paren, List<Expr> arguments)
			{
				this.callee = callee;
				this.paren = paren;
				this.arguments = arguments;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitCallExpr(this);
			}

			public readonly Expr callee;
			public readonly Token paren;
			public readonly List<Expr> arguments;
		}
		public class Get : Expr
		{
			public Get(Expr obj, Token name)
			{
				this.obj = obj;
				this.name = name;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitGetExpr(this);
			}

			public readonly Expr obj;
			public readonly Token name;
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
		public class Set : Expr
		{
			public Set(Expr obj, Token name, Expr value)
			{
				this.obj = obj;
				this.name = name;
				this.value = value;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitSetExpr(this);
			}

			public readonly Expr obj;
			public readonly Token name;
			public readonly Expr value;
		}
		public class Super : Expr
		{
			public Super(Token keyword, Token method)
			{
				this.keyword = keyword;
				this.method = method;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitSuperExpr(this);
			}

			public readonly Token keyword;
			public readonly Token method;
		}
		public class This : Expr
		{
			public This(Token keyword)
			{
				this.keyword = keyword;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitThisExpr(this);
			}

			public readonly Token keyword;
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
