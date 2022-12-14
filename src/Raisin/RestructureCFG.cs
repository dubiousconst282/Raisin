namespace Raisin;

using DistIL.Analysis;
using DistIL.IR;
using DistIL.Util;

//https://www.backerstreet.com/decompiler/creating_statements.php
public class RestructureCFG
{
    Dictionary<BasicBlock, StructuredBlock> _structuredBlocks = new();
    
    public static StructuredBlock Unflatten(MethodBody method)
    {
        var restr = new RestructureCFG();
        return restr.Unflatten(method.EntryBlock);
    }

    private StructuredBlock Unflatten(BasicBlock block)
    {
        if (_structuredBlocks.TryGetValue(block, out var structured)) {
            return structured;
        }
        _structuredBlocks[block] = structured = new StructuredBlock() {
            Kind = StructureKind.Flat,
            UnderlyingBlock = block,
            Succs = new StructuredBlock[block.NumSuccs]
        };

        int succIdx = 0;
        foreach (var succ in block.Succs) {
            structured.Succs[succIdx++] = Unflatten(succ);
        }
        UnflattenIf(structured);
        return structured;
    }

    private void UnflattenIf(StructuredBlock block)
    {
        if (block.Succs is not [var thenBlock, var elseBlock] || block.UnderlyingBlock.Last is not BranchInst) return;

        //BB_Head: goto cond ? BB_Then : BB_Else
        //BB_Then: goto BB_End;
        //BB_Else: goto BB_End;
        //
        //if (cond) { BB_Then } else { BB_Else }
        //BB_End
        if (
            thenBlock.Succs.Length == 1 && elseBlock.Succs.Length == 1 &&
            thenBlock.Succs[0] == elseBlock.Succs[0]
        ) {
            block.Kind = StructureKind.IfBranch;
            block.Succs = new[] { thenBlock.Succs[0] };
            block.Children = new[] { thenBlock, elseBlock };
            thenBlock.Succs = elseBlock.Succs = Array.Empty<StructuredBlock>();
        }
        //BB_Head: goto cond ? BB_Then : BB_Else
        //BB_Then: goto BB_Else;
        //
        //if (cond) { BB_Then }
        //BB_Else
        else if (thenBlock.Succs.Length == 1 && thenBlock.Succs[0] == elseBlock) {
            block.Kind = StructureKind.IfBranch;
            block.Succs = new[] { elseBlock };
            block.Children = new[] { thenBlock };
            thenBlock.Succs = Array.Empty<StructuredBlock>();
        }
        //BB_Head: goto cond ? BB_Then : BB_Else
        //BB_Else: goto BB_Then;
        //
        //if (!cond) { BB_Else }
        //BB_Then
        else if (elseBlock.Succs.Length == 1 && elseBlock.Succs[0] == thenBlock) {
            block.Kind = StructureKind.IfBranch;
            block.Succs = new[] { thenBlock };
            block.Children = new[] { elseBlock };
            elseBlock.Succs = Array.Empty<StructuredBlock>();
        }
    }
}

public class StructuredBlock
{
    public StructureKind Kind;
    public BasicBlock UnderlyingBlock = null!;
    public StructuredBlock[] Succs = null!;
    public StructuredBlock[]? Children = null!;
}
public enum StructureKind
{
    Flat, IfBranch,
    Loop, WhileLoop
}