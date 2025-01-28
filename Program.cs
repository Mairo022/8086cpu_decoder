// 1000100111011001
//       0 dir
//        1 width
//         11 cx MOD
//           011 bx REG
//              001 R/M

if (args.Length == 0) return;

using var fileReadStream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
var buffer = new byte[2];

Console.WriteLine("bits 16\n");

while (fileReadStream.Read(buffer, 0 , buffer.Length) > 0)
{
    var binaryInput = BytesToBinary(buffer);

    if (binaryInput[..6] != "100010") continue;

    var d = Convert.ToInt32(binaryInput[6].ToString(), 2);
    var w = Convert.ToInt32(binaryInput[7].ToString(), 2);
    var mod = Convert.ToInt32(binaryInput[8..10], 2);
    var reg = Convert.ToInt32(binaryInput[10..13], 2);
    var rm = Convert.ToInt32(binaryInput[13..], 2);

    string src;
    string dest;

    if (w == 1)
    {
        src = ((Width16Encodings) reg).ToString();
        dest = ((Width16Encodings) rm).ToString();
    }
    else
    {
        src = ((Width8Encodings) reg).ToString();
        dest = ((Width8Encodings) rm).ToString();
    }

    Console.WriteLine($"mov {dest}, {src}");
}

return;

static string BytesToBinary(byte[] bytes)
{
    var binaryString = string.Empty;
    
    foreach (byte b in bytes)
    {
        binaryString += Convert.ToString(b, 2).PadLeft(8, '0');
    }
    
    return binaryString;
}

enum Width8Encodings
{
    al = 0,
    cl = 1,
    dl = 2,
    bl = 3,
    ah = 4,
    ch = 5,
    dh = 6,
    bh = 7
}

enum Width16Encodings
{
    ax = 0,
    cx = 1,
    dx = 2,
    bx = 3,
    sp = 4,
    bp = 5,
    si = 6,
    di = 7
}