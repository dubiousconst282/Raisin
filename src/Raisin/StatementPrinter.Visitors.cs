namespace Raisin;

using System.Diagnostics;

using DistIL.IR;

public partial class StatementPrinter : InstVisitor
{
    void InstVisitor.Visit(BinaryInst inst)
    {
        var (sym, prec) = GetSymbol(inst.Op);
        PrintSubExpr(inst.Left);
        _output.Print($" {sym} ");
        PrintSubExpr(inst.Right);

        void PrintSubExpr(Value value)
        {
            bool needParen = value is BinaryInst sub && GetSymbol(sub.Op).Prec < prec && !IsStatement(sub);

            if (needParen) _output.Print("(");
            Accept(value);
            if (needParen) _output.Print(")");
        }
    }

    void InstVisitor.Visit(UnaryInst inst)
    {
        _output.Print(GetSymbol(inst.Op));
        Accept(inst.Value);
    }

    void InstVisitor.Visit(CompareInst inst)
    {
        Accept(inst.Left);
        _output.Print($" {GetSymbol(inst.Op)} ");
        Accept(inst.Right);
    }

    void InstVisitor.Visit(ConvertInst inst)
    {
        Debug.Assert(!inst.CheckOverflow && !inst.SrcUnsigned);
        _output.Print($"({inst.ResultType})");
        Accept(inst.Value);
    }

    void InstVisitor.Visit(LoadVarInst inst)
    {
        inst.Var.Print(_output);
    }

    void InstVisitor.Visit(StoreVarInst inst)
    {
        inst.Var.Print(_output);
        _output.Print(" = ");
        Accept(inst.Value);
    }

    void InstVisitor.Visit(VarAddrInst inst)
    {
        _output.Print("&");
        Accept(inst.Var);
    }

    void InstVisitor.Visit(LoadPtrInst inst)
    {
        Debug.Assert(!inst.Volatile && !inst.Unaligned);
        _output.Print($"*({inst.ElemType}*)");
        Accept(inst.Address);
    }

    void InstVisitor.Visit(StorePtrInst inst)
    {
        Debug.Assert(!inst.Volatile && !inst.Unaligned);
        _output.Print($"*({inst.ElemType}*)");
        Accept(inst.Address);
        _output.Print(" = ");
        Accept(inst.Value);
    }

    void InstVisitor.Visit(ArrayLenInst inst)
    {
        Accept(inst.Array);
        _output.Print(".Length");
    }

    void InstVisitor.Visit(LoadArrayInst inst)
    {
        Debug.Assert(inst.ElemType == inst.Array.ResultType.ElemType);
        Accept(inst.Array);
        _output.Print("[");
        Accept(inst.Index);
        _output.Print("]");
    }

    void InstVisitor.Visit(StoreArrayInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(ArrayAddrInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(LoadFieldInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(StoreFieldInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(FieldAddrInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(CallInst inst)
    {
        if (inst.IsStatic) {
            inst.Method.DeclaringType.Print(_output);
        } else {
            Accept(inst.Args[0]);
        }
        _output.Print($".{PrintToner.MethodName}{inst.Method.Name}(");
        int offset = inst.IsStatic ? 0 : 1;
        for (int i = offset; i < inst.NumArgs; i++) {
            if (i != offset) _output.Print(", ");
            Accept(inst.Args[i]);
        }
        _output.Print(")");
    }

    void InstVisitor.Visit(NewObjInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(FuncAddrInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(IntrinsicInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(ReturnInst inst)
    {
        _output.Print("return", PrintToner.Keyword);
        if (inst.HasValue) {
            _output.Print(" ");
            Accept(inst.Value);
        }
    }

    void InstVisitor.Visit(BranchInst inst)
    {
        if (inst.IsConditional) {
            _output.Print("if (");
            Accept(inst.Cond);
            _output.Print($") goto {inst.Then}; else goto {inst.Else}");
        } else {
            _output.Print($"{PrintToner.Keyword}goto {inst.Then}");
        }
    }

    void InstVisitor.Visit(SwitchInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(PhiInst inst)
    {
        _output.Print("φ", PrintToner.Keyword);
        _output.Print("(");
        for (int i = 0; i < inst.NumArgs; i++) {
            if (i != 0) _output.Print(", ");
            Accept(inst.GetValue(i));
        }
        _output.Print(")");
    }

    void InstVisitor.Visit(GuardInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(LeaveInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(ResumeInst inst)
    {
        throw new NotImplementedException();
    }

    void InstVisitor.Visit(ThrowInst inst)
    {
        throw new NotImplementedException();
    }

    private static (string Sym, int Prec) GetSymbol(BinaryOp op) => op switch {
        BinaryOp.Add or BinaryOp.FAdd => ("+", 10),
        BinaryOp.Sub or BinaryOp.FSub => ("-", 10),
        BinaryOp.Mul or BinaryOp.FMul => ("*", 20),
        BinaryOp.SDiv or BinaryOp.UDiv or BinaryOp.FDiv => ("/", 20),
        BinaryOp.SRem or BinaryOp.URem or BinaryOp.FRem => ("%", 20),
        BinaryOp.Or     => ("|", 4),
        BinaryOp.Xor    => ("^", 5),
        BinaryOp.And    => ("&", 6),
        //Cmp Eq/Ne     => 7
        //Cmp Rel       => 8
        BinaryOp.Shl    => ("<<", 9),
        BinaryOp.Shrl   => (">>>", 9),
        BinaryOp.Shra   => (">>", 9),
    };

    private static string GetSymbol(CompareOp op) => op switch {
        CompareOp.Eq => "==",
        CompareOp.Ne => "!=",
        CompareOp.Slt or CompareOp.Ult or CompareOp.FOlt => "<",
        CompareOp.Sgt or CompareOp.Ugt or CompareOp.FOgt => ">",
        CompareOp.Sle or CompareOp.Ule or CompareOp.FOle => "<=",
        CompareOp.Sge or CompareOp.Uge or CompareOp.FOge => ">="
    };

    private static string GetSymbol(UnaryOp op) => op switch {
        UnaryOp.Neg or UnaryOp.FNeg => "-",
        UnaryOp.Not => "~"
    };
}