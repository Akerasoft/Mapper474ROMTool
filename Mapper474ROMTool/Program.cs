// (C) Copyright 2023 Akerasoft
// Akeraiotitasoft LLC DBA Akerasoft
//
// Programmer: Robert Kolski, but Akerasoft holds the copyright.
// To contact me: robert.kolski@akerasoft.com
//
// Open Source Apache 2.0 License, see LICENSE file

using com.clusterrr.Famicom.Containers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapper474ROMTool
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("(C) Copyright 2023 Akerasoft");
            Console.WriteLine("Akeraiotitasoft LLC DBA Akerasoft");

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCommandLine(args);
            IConfiguration configuration = configurationBuilder.Build();
            string mode = configuration["mode"];
            string input = configuration["input"];
            string output = configuration["output"];
            string inputChrFilename = configuration["input-chr"];
            string inputPrgFilename = configuration["input-prg"];
            string strSubmapper = configuration["submapper"];
            string outputHeaderFilename = configuration["output-header"];
            string outputChrFilename = configuration["output-chr"];
            string outputPrgFilename = configuration["output-prg"];
            string strMirroring = configuration["mirroring"];
            string strRegion = configuration["region"];

            if (mode == "combine")
            {
                int submapper;
                if (!int.TryParse(strSubmapper, out submapper) || submapper < 0 || submapper > 1)
                {
                    System.Console.Error.WriteLine("--submapper should be 0 or 1");
                    return ShowUsage();
                }
                Timing region;
                if (!Enum.TryParse(strRegion, out region))
                {
                    System.Console.Error.WriteLine("--region is invalid");
                    return ShowUsage();
                }
                MirroringType mirroring;
                if (!Enum.TryParse(strMirroring, out mirroring))
                {
                    System.Console.Error.WriteLine("--mirroring is invalid");
                    return ShowUsage();
                }

                if (string.IsNullOrEmpty(inputChrFilename) || string.IsNullOrEmpty(inputPrgFilename) || string.IsNullOrEmpty(output))
                {
                    System.Console.Error.WriteLine("Missing parameter(s)");
                    return ShowUsage();
                }

                byte[] chr = System.IO.File.ReadAllBytes(inputChrFilename);
                if (chr.Length != 0x2000)
                {
                    System.Console.Error.WriteLine("Wrong length of CHR ROM, should by 8KB.");
                }

                byte[] prg = System.IO.File.ReadAllBytes(inputPrgFilename);
                if (chr.Length != 0x10000)
                {
                    System.Console.Error.WriteLine("Wrong length of PRG ROM, should be 64KB.  The first 16KB is skipped while storing but must be present.");
                }

                byte[] savePrg = new byte[0xC000];
                Array.Copy(prg, 0x4000, savePrg, 0x0000, 0xC000);

                NesFile nesFile = new NesFile();
                nesFile.Version = NesFile.iNesVersion.NES20;
                nesFile.Mapper = 474;
                nesFile.CHR = chr;
                nesFile.PRG = savePrg;
                nesFile.Submapper = (byte)submapper;
                nesFile.Console = NesFile.ConsoleType.Normal;
                nesFile.ExtendedConsole = NesFile.ExtendedConsoleType.RegularNES;
                nesFile.Region = region;
                nesFile.Mirroring = mirroring;
                nesFile.Save(output);
                return 0;
            }
            else if (mode == "split")
            {
                if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(outputChrFilename) || string.IsNullOrEmpty(outputPrgFilename) || string.IsNullOrEmpty(outputHeaderFilename))
                {
                    System.Console.Error.WriteLine("Missing parameter(s)");
                    return ShowUsage();
                }

                NesFile nesFile = NesFile.FromFile(input);
                if (nesFile.Mapper == 474)
                {
                    if (nesFile.Submapper < 0 || nesFile.Submapper > 1)
                    {
                        System.Console.Error.WriteLine("submapper should be 0 or 1");
                        return 1;
                    }

                    byte[] header = nesFile.ToBytes().Take(16).ToArray();
                    System.IO.File.WriteAllBytes(outputHeaderFilename, header);
                    System.IO.File.WriteAllBytes(outputChrFilename, nesFile.CHR);
                    byte[] prgpad = new byte[0x4000];
                    Array.Fill(prgpad, (byte)0);
                    List<byte> prg = new List<byte>(prgpad);
                    prg.AddRange(nesFile.PRG);
                    System.IO.File.WriteAllBytes(outputPrgFilename, prg.ToArray());
                    return 0;
                }
                else
                {
                    System.Console.Error.WriteLine("Mapper is not supported.");
                    return 1;
                }
            }
            else
            {
                return ShowUsage();
            }
        }

        static int ShowUsage()
        {
            System.Console.Out.WriteLine("Usage:");
            System.Console.Out.WriteLine("Mapper474ROMTool --mode=combine|split [extra required args]");
            System.Console.Out.WriteLine("Mapper474ROMTool --mode=combine --input-chr=chr.bin --input-prg=prg.bin --output=myrom.nes --submapper=0|1 --region=Ntsc|Pal|Dendy|Multiple --mirroring=Horizontal|Vertical");
            System.Console.Out.WriteLine("Mapper474ROMTool --mode=split --input=myrom.nes --output-header=header.bin --output-chr=chr.bin --output-prg=prg.bin");
            return 1;
        }
    }
}
