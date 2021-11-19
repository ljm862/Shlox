using System;
using System.Collections.Generic;
using LoxInterpreter.Lexing;

namespace LoxInterpreter
{
	public abstract class Stmt
	{
		public interface IVisitor<T>
		{
			T VisitBlockStmt(Block stmt);
			T VisitExpressionStmt(Expression stmt);
			T VisitIfStmt(If stmt);
			T VisitPrintStmt(Print stmt);
			T VisitVarStmt(Var stmt);
			T VisitWhileStmt(While stmt);
		}
		public class Block : Stmt
		{
			public Block(List<Stmt> statements)
			{
				this.statements = statements;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitBlockStmt(this);
			}

			public readonly List<Stmt> statements;
		}
		public class Expression : Stmt
		{
			public Expression(Expr expression)
			{
				this.expression = expression;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitExpressionStmt(this);
			}

			public readonly Expr expression;
		}
		public class If : Stmt
		{
			public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
			{
				this.condition = condition;
				this.thenBranch = thenBranch;
				this.elseBranch = elseBranch;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitIfStmt(this);
			}

			public readonly Expr condition;
			public readonly Stmt thenBranch;
			public readonly Stmt elseBranch;
		}
		public class Print : Stmt
		{
			public Print(Expr expression)
			{
				this.expression = expression;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitPrintStmt(this);
			}

			public readonly Expr expression;
		}
		public class Var : Stmt
		{
			public Var(Token name, Expr initializer)
			{
				this.name = name;
				this.initializer = initializer;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitVarStmt(this);
			}

			public readonly Token name;
			public readonly Expr initializer;
		}
		public class While : Stmt
		{
			public While(Expr condition, Stmt body)
			{
				this.condition = condition;
				this.body = body;
			}

			public override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitWhileStmt(this);
			}

			public readonly Expr condition;
			public readonly Stmt body;
		}

		public abstract T Accept<T>(IVisitor<T> visitor);
	}
}
