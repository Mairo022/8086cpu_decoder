namespace CpuSimulation;

public static class Encodings
{
    public static readonly string[] ModMem = 
    [
        "bx + si",
        "bx + di",
        "bp + si",
        "bp + di",
        "si",
        "di",
        "bp",
        "bx",
    ];

    public static readonly string[] ModRegOnly8 =
    [
        "al",
        "cl",
        "dl",
        "bl",
        "ah",
        "ch",
        "dh",
        "bh"
    ];

    public static readonly string[] ModRegOnly16 =
    [
        "ax",
        "cx",
        "dx",
        "bx",
        "sp",
        "bp",
        "si",
        "di"
    ];
}
