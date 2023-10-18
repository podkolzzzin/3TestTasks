// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;


Console.WriteLine("Hello, World!");

Console.WriteLine(Vector256<int>.IsSupported);
Console.WriteLine(Vector256<int>.Count);

Console.WriteLine(Vector<int>.IsSupported);
Console.WriteLine(Vector<int>.Count);