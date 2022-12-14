namespace Raisin;

using DistIL.Analysis;
using DistIL.IR;
using DistIL.Util;

public class CodeGenerator
{
    MethodBody _method;
    PrintContext _output;
    StatementPrinter _stmtPrinter;

    public CodeGenerator(MethodBody method, PrintContext output)
    {
        _method = method;
        _output = output;
        _stmtPrinter = new StatementPrinter(output, new ForestAnalysis(method));
    }

    public void Generate()
    {
        var block = RestructureCFG.Unflatten(_method);
        PrintBlock(block);
    }

    private void PrintBlock(StructuredBlock block, bool excludeLastStmt = false)
    {
        var underBlock = block.UnderlyingBlock;
        var lastInst = excludeLastStmt || block.Kind == StructureKind.IfBranch 
            ? underBlock.Last.Prev! 
            : underBlock.Last;
        _output.PrintLine($"//{block.UnderlyingBlock}");
        PrintStmts(underBlock.First, lastInst);

        if (block.Kind == StructureKind.IfBranch) {
            var branch = (BranchInst)underBlock.Last;

            _output.PrintLine();
            _output.Print("if", PrintToner.Keyword);
            _output.Print(" (");
            _stmtPrinter.Accept(branch.Cond!);
            _output.Push(") {");

            PrintBlock(block.Children![0], true);
            _output.Pop("}");
            
            if (block.Children!.Length >= 2) {
                _output.Push(" else {");
                PrintBlock(block.Children![1], true);
                _output.Pop("}");
            }
            _output.PrintLine();
        }

        foreach (var succ in block.Succs) {
            PrintBlock(succ);
        }
    }
    private void PrintStmts(Instruction first, Instruction last)
    {
        for (var inst = first; ; inst = inst.Next!) {
            if (_stmtPrinter.IsStatement(inst)) {
                _stmtPrinter.PrintStmt(inst);
            }
            if (inst == last) break;
        }
    }
}