﻿using System;
using System.Text;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Order;
using System.IO;
using System.Linq;
using Force.Crc32;

namespace BenchmarkLab
{
    public class Program
    {
        static void Main(string[] args)
        {
            /*
            Console.WriteLine("Hello World!");
            Console.WriteLine("Old SummarizeSize Result: " + new Tool().SummarizeSize((long)(uint.MaxValue * 0.987654321)) + " " + new Tool().SummarizeSize((long)(int.MaxValue * 0.987654321)));
            Console.WriteLine("SummarizeSizeSimple Result: " + new Tool().SummarizeSizeSimple(uint.MaxValue * 0.987654321) + " " + new Tool().SummarizeSizeSimple((int.MaxValue * 0.987654321)));
            Console.WriteLine("SummarizeSizeSimpleWithDecimal Result: " + new Tool().SummarizeSizeSimple(uint.MaxValue * 0.987654321, 9) + " " + new Tool().SummarizeSizeSimple(int.MaxValue * 0.987654321, 9));

            Console.WriteLine(new Tool().SummarizeSizeSimple(uint.MaxValue));
            Console.WriteLine(new Tool().Digits_FindCountWithPrec(12345678.87654));
            */

            CRCTest a = new CRCTest();
            byte[] data = File.ReadAllBytes(@"C:\Users\neon-nyan\Downloads\yoimiya_ayaka.png");
            FileStream stream = new FileStream(@"C:\Users\neon-nyan\Downloads\yoimiya_ayaka.png", FileMode.Open, FileAccess.Read);

            string hash = a.BytesToCRC32Simple(data);
            Console.WriteLine(hash);
            hash = a.BytesToCRC32Simple(stream);
            Console.WriteLine(hash);
            hash = a.BytesToCRC32(data);
            Console.WriteLine(hash);

            BenchmarkRunner.Run<CRCTest>();
        }
    }


    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class CRCTest
    {
        byte[] data = File.ReadAllBytes("sample.png");
        FileStream stream = new FileStream("sample.png", FileMode.Open, FileAccess.Read);
        Crc32Algorithm CRCEncoder = new Crc32Algorithm();

        [Benchmark]
        public void BytesToCRC32Simple() => BytesToCRC32Simple(data);

        [Benchmark]
        public void BytesToCRC32SimpleStream() => BytesToCRC32Simple(stream);
        public string BytesToCRC32Simple(in byte[] buffer) => BytesToHex(CRCEncoder.ComputeHash(new MemoryStream(buffer)));
        public string BytesToCRC32Simple(in Stream buffer) => BytesToHex(CRCEncoder.ComputeHash(buffer));

        [Benchmark]
        public void BytesToCRC32() => BytesToCRC32(data);
        public string BytesToCRC32(byte[] buffer)
        {
            Crc32Algorithm crc = new Crc32Algorithm();
            string hash = String.Empty;
            using (MemoryStream stream = new MemoryStream(buffer))
                foreach (byte a in crc.ComputeHash(stream)) hash += a.ToString("x2").ToLower();
            return hash;
        }
        public string BytesToHex(in ReadOnlySpan<byte> bytes) => Convert.ToHexString(bytes);
    }

    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class Tool
    {

        //[Benchmark]
        public void numberTest_If()
        {
            Digits_IfChain(int.MaxValue);
        }

        //[Benchmark]
        public void numberTest_Log10()
        {
            Digits_Log10(int.MaxValue);
        }

        //[Benchmark]
        public void numberTest_Log10Simplified()
        {
            Digits_Log10Simplified(int.MaxValue);
        }

        //[Benchmark]
        public void numberTest_FindCountWithPrec()
        {
            Digits_FindCountWithPrec(12345678.87654321);
        }

        public long Digits_FindCountWithPrec(double n) => (long)((n % 1) * 100);

        public long Digits_Log10Simplified(double n) => (long)Math.Log10(n) + 1;

        public int Digits_Log10(long n) =>
        n == 0L ? 1 : (n > 0L ? 1 : 2) + (int)Math.Log10(Math.Abs((double)n));

        public int Digits_IfChain(long n)
        {
            if (n >= 0)
            {
                if (n < 10L) return 1;
                if (n < 100L) return 2;
                if (n < 1000L) return 3;
                if (n < 10000L) return 4;
                if (n < 100000L) return 5;
                if (n < 1000000L) return 6;
                if (n < 10000000L) return 7;
                if (n < 100000000L) return 8;
                if (n < 1000000000L) return 9;
                if (n < 10000000000L) return 10;
                if (n < 100000000000L) return 11;
                if (n < 1000000000000L) return 12;
                if (n < 10000000000000L) return 13;
                if (n < 100000000000000L) return 14;
                if (n < 1000000000000000L) return 15;
                if (n < 10000000000000000L) return 16;
                if (n < 100000000000000000L) return 17;
                if (n < 1000000000000000000L) return 18;
                return 19;
            }
            else
            {
                if (n > -10L) return 2;
                if (n > -100L) return 3;
                if (n > -1000L) return 4;
                if (n > -10000L) return 5;
                if (n > -100000L) return 6;
                if (n > -1000000L) return 7;
                if (n > -10000000L) return 8;
                if (n > -100000000L) return 9;
                if (n > -1000000000L) return 10;
                if (n > -10000000000L) return 11;
                if (n > -100000000000L) return 12;
                if (n > -1000000000000L) return 13;
                if (n > -10000000000000L) return 14;
                if (n > -100000000000000L) return 15;
                if (n > -1000000000000000L) return 16;
                if (n > -10000000000000000L) return 17;
                if (n > -100000000000000000L) return 18;
                if (n > -1000000000000000000L) return 19;
                return 20;
            }
        }

        //[Benchmark]
        public void SummarizeSize()
        {
            for (ushort i = ushort.MaxValue; i > 0; i--)
                SummarizeSize((long)(i * 0.987654321));
        }

        [Benchmark]
        public void SummarizeSizeSimpleWithDecimal()
        {
            for (ushort i = ushort.MaxValue; i > 0; i--)
                SummarizeSizeSimple(i * 0.987654321, 9);
        }

        [Benchmark]
        public void SummarizeSizeSimple()
        {
            for (ushort i = ushort.MaxValue; i > 0; i--)
                SummarizeSizeSimple(i * 0.987654321);
        }

        public string SummarizeSize(Int64 value, int decimalPlaces = 2)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SummarizeSize(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
        string[] SizeSuffixes = { " B", " KB", " MB", " GB", " TB", " PB", " EB", " ZB", " YB" };

        public string SummarizeSizeSimple(double value, int decimalPlaces = 2)
        {
            byte mag = (byte)Math.Log(value, 1000);
            return $"{Math.Round((value / (1L << (mag * 10))), decimalPlaces)}{SizeSuffixes[mag]}";
        }
    }
}
