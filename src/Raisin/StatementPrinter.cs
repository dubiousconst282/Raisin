namespace Raisin;

using DistIL.Analysis;
using DistIL.IR;
using DistIL.Util;

public partial class StatementPrinter : InstVisitor
{
    readonly PrintContext _output;
    readonly ForestAnalysis _forest;

    public StatementPrinter(PrintContext output, ForestAnalysis forest)
    {
        _output = output;
        _forest = forest;
    }

    public bool IsStatement(Instruction inst)
    {
        return _forest.IsTreeRoot(inst) &&
                !(inst is CompareInst && inst.NumUses == 1 && inst.Users().First() is BranchInst);
    }

    public void PrintStmt(Instruction inst)
    {
        if (inst.HasResult) {
            inst.ResultType.Print(_output);
            _output.Print(" ");
            inst.PrintAsOperand(_output);
            _output.Print(" = ");
        }
        inst.Accept(this);
        _output.PrintLine(";");
    }

    public void Accept(Value value)
    {
        switch (value) {
            case Instruction c: {
                if (IsStatement(c)) {
                    c.PrintAsOperand(_output);
                } else {
                    c.Accept(this);
                }
                break;
            }
            case Const or Argument or Variable: {
                value.Print(_output);
                break;
            }
            default: throw new NotImplementedException();
        }
    }
}