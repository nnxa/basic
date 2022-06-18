if (args.Length != 1)
{
    Console.WriteLine("Usage: " + AppDomain.CurrentDomain.FriendlyName + " <output directory>");
    return 64;
}

var outputDir = args[0];

DefineAst(outputDir, "Expr", new[]
{
    "Binary   : Expr Left, Token Operator, Expr Right",
    "Grouping : Expr Expression",
    "Literal  : object Value",
    "Unary    : Token Operator, Expr Right",
    "Variable : Token Name, List<Expr> Parameters"
});

DefineAst(outputDir, "Stmt", new[]
{
    "Assign : Expr.Variable Variable, Expr Value",
    "Dim    : List<Expr.Variable> Variable",
    "For    : Token Stmt, Expr.Variable Variable, Expr Start, Expr Target, Expr Step, List<Stmt> Block",
    "Goto   : Token Stmt, Expr Label",
    "Gosub  : Token Stmt, Expr Label",
    "If     : Expr Condition, List<Stmt> IfTrue, List<Stmt> IfFalse",
    "New    : ",
    "Next   : Stmt.For ForStatement",
    "On     : Token Name, Expr Expression, Token Keyword, List<Expr> Targets",
    "Print  : PrintUserStatement UserStatement, Token Stmt, List<Expr> Expressions, bool NewLine",
    "Read   : List<Expr.Variable> Variables",
    "Return : Token Stmt",
    "Run    : ",
    "Let    : Token Name, Expr Initializer",
    "Noop   : ",
    "End    : ",
    "User   : Token Name, GenericUserStatement UserStatement, int Pattern, List<Expr> Expressions"
}); ;

return 0;

void DefineAst(string outputDir, string baseName, IEnumerable<string> types, bool @void = false)
{
    var path = Path.Combine(outputDir, baseName + ".cs");
    using var textWriter = File.CreateText(path);
    textWriter.WriteLine( "namespace Basic.Interpreter");
    textWriter.WriteLine( "{");
    textWriter.WriteLine($"    internal abstract class {baseName}");
    textWriter.WriteLine( "    {");
    textWriter.WriteLine();

    if (baseName == "Stmt")
    {
        textWriter.WriteLine("        private static Stmt.End _end = new Stmt.End();");
        textWriter.WriteLine("        public int LineNumber { get; set; }");
        textWriter.WriteLine("        public Stmt NextStatement { get; private set; } = _end;");
        textWriter.WriteLine();
    }

    var returnType = @void ? "void" : "T";
    var generic = @void ? "" : "<T>";

    DefineVisitor(textWriter, baseName, types, @void);

    foreach (var type in types)
    {
        var split = type.Split(':');
        var className = split[0].Trim();
        var fields = split[1].Trim();
        var settableFields = split.Length >= 3 ? split[2].Trim() : "";
        DefineType(textWriter, baseName, className, fields, settableFields, @void);
    }

    textWriter.WriteLine($"        public abstract {returnType} Accept{generic}(Visitor{generic} visitor);");


    if (baseName == "Stmt")
        textWriter.WriteLine($"        public abstract void SetNextStatement(Stmt nextStatement);");
    textWriter.WriteLine();

    textWriter.WriteLine( "    }");
    textWriter.WriteLine( "}");
}

void DefineType(StreamWriter writer, string baseName, string className,
                string fieldList, string propertyList, bool @void)
{

    var returnType = @void ? "void" : "T";
    var generic = @void ? "" : "<T>";

    writer.WriteLine("        public class " + className + " : " + baseName);
    writer.WriteLine("        {");
    writer.WriteLine();
    
    var fields = fieldList
        .Split(",")
        .Select(x => x.Trim())
        .Where(x => x is not null && x != "")
        .ToList();

    var fieldTypes = fields.Select(x => x.Split(' ')[0].Trim()).ToList();
    var fieldsPascal = fields.Select(x => x.Split(' ')[1].Trim()).ToList();
    var fieldsCamel = fieldsPascal
        .Select(x => x.Substring(0, 1).ToLower() + x.Substring(1))
        .Select(x => x == "operator" ? "@operator" : x)
        .ToList();

    var properties = propertyList.Split(",").Select(x => x.Trim()).Where(x => x is not null && x != "");

    foreach (var prop in properties)
        writer.WriteLine("            public " + prop + " { get; set; }");

    var parameters = string.Join(", ", Enumerable.Range(0, fields.Count).Select(i => fieldTypes[i] + " " + fieldsCamel[i]));

    for (int i = 0; i < fields.Count(); i++)
    {
        writer.WriteLine("            public " + fieldTypes[i] + " " + fieldsPascal[i] + " { get; }");
    }

    writer.WriteLine();

    writer.WriteLine("            public " + className + " (" + parameters + ")");
    writer.WriteLine("            {");

    for (int i = 0; i < fields.Count(); i++)
    {
        writer.WriteLine("                " + fieldsPascal[i] + " = " + fieldsCamel[i] + ";");
    }

    writer.WriteLine("            }");
    writer.WriteLine();
    writer.WriteLine($"            public override {returnType} Accept{generic}(Visitor{generic} visitor)");
    writer.WriteLine("            {");
    writer.WriteLine("                " + (@void ? "" : "return ") + "visitor.Visit" + className + baseName + "(this);");
    writer.WriteLine("            }");
    writer.WriteLine("");

    if (baseName == "Stmt")
    {
        writer.WriteLine("            public override void SetNextStatement(Stmt nextStatement)");
        writer.WriteLine("            {");
        writer.WriteLine("                NextStatement = nextStatement;");
        if (className == "If")
        {
            writer.WriteLine("                if (IfTrue.Any()) IfTrue.Last().SetNextStatement(nextStatement);");
            writer.WriteLine("                if (IfFalse.Any()) IfFalse.Last().SetNextStatement(nextStatement);");
        }
        writer.WriteLine("            }");
        writer.WriteLine();
    }
    writer.WriteLine("        }");
    writer.WriteLine();
}

void DefineVisitor(StreamWriter writer, string baseName,
                   IEnumerable<string> types, bool @void)
{

    var returnType = @void ? "void" : "T";
    var generic = @void ? "" : "<T>";

    writer.WriteLine($"        public interface Visitor{generic}");
    writer.WriteLine("        {");
    foreach (var type in types)
    {
        var typeName = type.Split(':')[0].Trim();
        writer.WriteLine($"            {returnType} Visit" + typeName + baseName + " (" + typeName + " " + baseName.ToLower() + ");");
    }
    writer.WriteLine("        }");
    writer.WriteLine();
}