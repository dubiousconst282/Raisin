# Raisin
Toy pseudo-C#/IL decompiler based on DistIL

Does not support loops and barely works yet. Sample output just for looks:

```cs
//BB_01
$a_x = #x;
$a_y = #y;
string r6 = &$a_x.ToString().Substring(0, 1);

if (String.op_Equality(r6, &$a_x.ToString().Substring(0, 1)) == 0U) {
  //BB_12
  $a_y = $a_y + 1;
}
//BB_17
int r26 = $a_x * 2 * $a_x + $a_y * 4 * $a_y;
int r35 = $a_x * 2 * $a_x - $a_y * 4 * $a_y;
return Math.Abs(r26 - r35) + Math.Abs(r26 - r35);
```

Corresponding IR:

```cs
BB_01:
  stvar $a_x, #x
  stvar $a_y, #y
  int& r4 = varaddr $a_x
  string r5 = call Int32::ToString(this: r4)
  string r6 = callvirt String::Substring(this: r5, int: 0, int: 1)
  string r7 = call Int32::ToString(this: r4)
  string r8 = callvirt String::Substring(this: r7, int: 0, int: 1)
  bool r9 = call String::op_Equality(string: r6, string: r8)
  bool r10 = cmp.eq r9, 0U
  goto r10 ? BB_17 : BB_12
BB_12: //preds: BB_01
  int r13 = ldvar $a_y
  int r14 = add r13, 1
  stvar $a_y, r14
  goto BB_17
BB_17: //preds: BB_12 BB_01
  int r18 = ldvar $a_x
  int r19 = mul r18, 2
  int r20 = ldvar $a_x
  int r21 = mul r19, r20
  int r22 = ldvar $a_y
  int r23 = mul r22, 4
  int r24 = ldvar $a_y
  int r25 = mul r23, r24
  int r26 = add r21, r25
  int r27 = ldvar $a_x
  int r28 = mul r27, 2
  int r29 = ldvar $a_x
  int r30 = mul r28, r29
  int r31 = ldvar $a_y
  int r32 = mul r31, 4
  int r33 = ldvar $a_y
  int r34 = mul r32, r33
  int r35 = sub r30, r34
  int r36 = sub r26, r35
  int r37 = call Math::Abs(int: r36)
  int r38 = sub r26, r35
  int r39 = call Math::Abs(int: r38)
  int r40 = add r37, r39
  ret r40
```