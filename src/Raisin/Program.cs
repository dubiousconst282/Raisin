using DistIL.AsmIO;
using DistIL.Frontend;
using DistIL.IR;
using DistIL.IR.Utils;
using DistIL.Passes;

using Raisin;

if (args.Length == 0) {
    Console.WriteLine($"Arguments: <input module path>");
    return;
}

var resolver = new ModuleResolver();
resolver.AddTrustedSearchPaths();
var module = resolver.Load(args[0]);

foreach (var method in module.AllMethods()) {
    if (method.ILBody == null || method.Name != "VN1") continue;
    try {
        method.Body = ILImporter.ImportCode(method);

        //new SsaTransform().Run(new MethodTransformContext(method.Body));
        //new SimplifyInsts(module).Run(new MethodTransformContext(method.Body));
        IRPrinter.ExportPlain(method.Body, "logs/ir.txt");

        using var outStream = File.CreateText("logs/decomp.txt");
        var printer = new PrintContext(outStream, method.Body.GetSymbolTable());
        var codeGen = new CodeGenerator(method.Body, printer);
        codeGen.Generate();
    } catch (Exception ex) {
        Console.WriteLine($"FailImp: {method} {ex.Message}");
    }
}