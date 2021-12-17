import sys

def defineVisitor(file, base_name, types):
    file.write("    public interface IVisitor<T> {\n")

    for type in types:
        type_name = type.split(":")[0].strip()
        file.write("    T Visit" + type_name + base_name + "(" + type_name + " " + base_name.lower() + ");\n")

    file.write("    }\n")

def defineType(file, base_name, class_name, field_list):
    file.write(" public class " + class_name + " : " + base_name + " {\n")

    file.write("    public " + class_name + "(" + field_list + ") {\n")

    fields = field_list.split(", ")
    for field in fields:
        name = field.split(" ")[1]
        file.write("    this." + name + " = " + name + ";\n")

    file.write("    }\n")

    file.write("\n")
    file.write("    public override T Accept<T>(IVisitor<T> visitor) {\n")
    file.write("        return visitor.Visit" + class_name + base_name + "(this);\n")
    file.write("    }\n")

    file.write("\n")
    for field in fields:
        file.write("    public readonly " + field + ";\n")

    file.write("    }\n")

def defineAst(output_directory, base_name, types):
    with open(output_directory + "/" + base_name + ".cs", "w") as file:
        file.write("using System;\n")
        file.write("using System.Collections.Generic;\n")
        file.write("using LoxInterpreter.Lexing;\n")
        file.write("\n")
        file.write("namespace LoxInterpreter\n")
        file.write("{\n")
        file.write("public abstract class " + base_name + " {\n")

        defineVisitor(file, base_name, types)

        for type in types:
            splits = type.split(":")
            class_name = splits[0].strip()
            fields = splits[1].strip()
            defineType(file, base_name, class_name, fields)

        file.write("\n")
        file.write("    public abstract T Accept<T>(IVisitor<T> visitor);\n")

        file.write("}\n")
        file.write("}\n")
        file.close()


def main(argv):
    if(len(argv) != 2):
        print("Usage: GenerateAst.py <output_directory>")
        sys.exit(64)
    output_directory = argv[1]
    expr_types = ["Assign : Token name, Expr value",
             "Binary : Expr left, Token oper, Expr right",
             "Call : Expr callee, Token paren, List<Expr> arguments",
             "Grouping : Expr expression",
             "Literal : Object value",
             "Logical : Expr left, Token oper, Expr right",
             "Unary : Token oper, Expr right",
             "Variable : Token name"]
    defineAst(output_directory, "Expr", expr_types)

    stmt_types = ["Block : List<Stmt> statements",
                "Expression : Expr expression",
                "Function : Token name, List<Token> parameters, List<Stmt> body",
                "If : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "Print : Expr expression",
                "Return : Token keyword, Expr value",
                "Var : Token name, Expr initializer",
                "While : Expr condition, Stmt body"]
    defineAst(output_directory, "Stmt", stmt_types)

if __name__ == "__main__":
    main(sys.argv)
