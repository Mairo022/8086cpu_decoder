﻿using static CpuDecoder8086.Encodings;

if (args.Length == 0) return;

using var fileReadStream = new FileStream(args[0], FileMode.Open, FileAccess.Read);
var buffer = new byte[1];

Console.WriteLine("bits 16\n");
    
var src = string.Empty;
var dest = string.Empty;

while (fileReadStream.Read(buffer, 0 , 1) > 0)
{
    var binaryInput = BytesToBinaryStr(buffer);

    // Reg & mem
    if (binaryInput[..6] == "100010")
    {
        var localBuffer = new byte[] { buffer[0], 0 };
        fileReadStream.ReadExactly(localBuffer, 1, 1);
        
        var binaryStr = BytesToBinaryStr(localBuffer);
        var d = Convert.ToInt16(binaryStr[6].ToString(), 2);
        var w = Convert.ToInt16(binaryStr[7].ToString(), 2);
        var mod = Convert.ToInt16(binaryStr[8..10], 2);
        var reg = Convert.ToInt16(binaryStr[10..13], 2);
        var rm = Convert.ToInt16(binaryStr[13..], 2);
        
        switch (mod)
        {
            case 0b00:
                src = d == 1 
                    ? AddressStr(ModMem[rm])
                    : w == 1 ? ModRegOnly16[reg] : ModRegOnly8[reg];
                dest = d == 1
                    ? w == 1 ? ModRegOnly16[reg] : ModRegOnly8[reg]
                    : AddressStr(ModMem[rm]);
                
                if (rm == 0b110)
                {
                    fileReadStream.ReadExactly(localBuffer, 0, 2);
                    if (d == 1) src = $"[{BitConverter.ToInt16(localBuffer, 0).ToString()}]";
                    if (d == 0) dest = $"[{BitConverter.ToInt16(localBuffer, 0).ToString()}]";
                }
                break;
            case 0b01:
                fileReadStream.ReadExactly(localBuffer, 0, 1);
                var disp8 = (sbyte) localBuffer[0];
                
                src = d == 1
                    ? AddressStr(ModMem[rm], disp8)
                    : w == 1 ? ModRegOnly16[reg] : ModRegOnly8[reg];
                dest = d == 1
                    ? w == 1 ? ModRegOnly16[reg] : ModRegOnly8[reg]
                    : AddressStr(ModMem[rm], disp8);
                break;
            case 0b10:
                fileReadStream.ReadExactly(localBuffer, 0, 2);
                var disp16 = BitConverter.ToInt16(localBuffer, 0);
                
                src = d == 1 
                    ? AddressStr(ModMem[rm], disp16)
                    : w == 1 ? ModRegOnly16[reg] : ModRegOnly8[reg];
                dest = d == 1
                    ? w == 1 ? ModRegOnly16[reg] : ModRegOnly8[reg]
                    : AddressStr(ModMem[rm], disp16);
                break;
            case 0b11:
                src = w == 1 ? ModRegOnly16[reg] : ModRegOnly8[reg];
                dest = w == 1 ? ModRegOnly16[rm] : ModRegOnly8[rm];
                break;
        }
    }
    
    // Immediate to reg/mem
    else if (binaryInput[..7] == "1100011")
    {
        var localBuffer = new byte[] { buffer[0], 0, 0, 0 };
        fileReadStream.ReadExactly(localBuffer, 1, 1);
        
        var binaryStr = BytesToBinaryStr(localBuffer[0..2]);
        var w = Convert.ToInt16(binaryStr[7].ToString(), 2);
        var mod = Convert.ToInt16(binaryStr[8..10], 2);
        var rm = Convert.ToInt16(binaryStr[13..], 2);

        switch (mod)
        {
            case 0b00:
                fileReadStream.ReadExactly(localBuffer, 0, w+1);
                
                var data0 = w == 0 
                    ? (sbyte) localBuffer[0] 
                    : BitConverter.ToInt16(localBuffer, 0);
                
                src = w == 0 
                    ? $"byte {data0}" 
                    : $"word {data0}";
                
                dest = AddressStr(ModMem[rm]);
                break;
            case 0b01:
                fileReadStream.ReadExactly(localBuffer, 0, w+1+mod);
                var disp8 = (sbyte) localBuffer[0];
                
                var data1 = w == 0 
                    ? (sbyte) localBuffer[1] 
                    : BitConverter.ToInt16(localBuffer, 1);
                
                src = w == 0
                    ? $"byte {data1}" 
                    : $"word {data1}";
                dest = AddressStr(ModMem[rm], disp8);
                
                break;
            case 0b10:
                fileReadStream.ReadExactly(localBuffer, 0, w+1+mod);
                var disp16 = BitConverter.ToInt16(localBuffer[..2], 0);

                var data2 = w == 0 
                    ? (sbyte) localBuffer[2] 
                    : BitConverter.ToInt16(localBuffer, 2);
                
                src = w == 0
                    ? $"byte {data2}" 
                    : $"word {data2}";
                dest = AddressStr(ModMem[rm], disp16);
                break;
        }
    }

    // Immediate to reg
    else if (binaryInput[..4] == "1011")
    {
        var localBuffer = new byte[2];
        
        var w = Convert.ToInt16(binaryInput[4].ToString(), 2);
        var reg = Convert.ToInt16(binaryInput[5..], 2);

        if (w == 0)
        {
            fileReadStream.ReadExactly(localBuffer, 0, 1);
            
            src = Convert.ToString(localBuffer[0]);
            dest = ModRegOnly8[reg];
        }
        else
        {
            fileReadStream.ReadExactly(localBuffer, 0, 2);
            var data = BitConverter.ToInt16(localBuffer, 0);

            src = Convert.ToString(data);
            dest = ModRegOnly16[reg];
        }
    }

    // Mem to acc
    else if (binaryInput[..7] == "1010000")
    {
        var localBuffer = new byte[2];
        var w = Convert.ToInt16(binaryInput[7].ToString(), 2);
        
        fileReadStream.ReadExactly(localBuffer, 0, w+1);

        src = w == 0
            ? localBuffer[0].ToString()
            : $"[{BitConverter.ToInt16(localBuffer, 0).ToString()}]";
        
        dest = w == 0
            ? ModRegOnly8[0]
            : ModRegOnly16[0];
    }
    
    // Acc to mem
    else if (binaryInput[..7] == "1010001")
    {
        var localBuffer = new byte[2];
        var w = Convert.ToInt16(binaryInput[7].ToString(), 2);
        
        fileReadStream.ReadExactly(localBuffer, 0, w+1);
        
        src = w == 0
            ? ModRegOnly8[0]
            : ModRegOnly16[0];

        dest = w == 0
            ? localBuffer[0].ToString()
            : $"[{BitConverter.ToInt16(localBuffer, 0).ToString()}]";
    }
    
    Console.WriteLine($"mov {dest}, {src}");
}

return;

static string BytesToBinaryStr(byte[] bytes)
{
    var binaryString = string.Empty;
    
    foreach (byte b in bytes)
    {
        binaryString += Convert.ToString(b, 2).PadLeft(8, '0');
    }
    
    return binaryString;
}

static string AddressStr(string addr, int disp = 0)
{
    return disp == 0 ? $"[{addr}]" : $"[{addr} + {disp}]";
}
