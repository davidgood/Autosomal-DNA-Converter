using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.IO.Compression;

namespace atDNA_Conv
{
    class Program
    {
        public const int TYPE_FTDNA = 0;
        public const int TYPE_23ANDME = 1;
        public const int TYPE_ANCESTRY = 2;
        public const int TYPE_DECODEME = 3;
        public const int TYPE_GENO2 = 4;
        public const int TYPE_PLINK = 5;
        public const int TYPE_EIGENSTRAT = 6;

        public static string[] TYPE = { "ftdna", "23andme", "ancestry", "decodeme", "geno2", "plink", "eigenstrat" };

        public static void printLogo()
        {
            Console.WriteLine("+-------------------------------------------+");
            Console.WriteLine("| Product: Autosomal DNA Converter          |");
            Console.WriteLine("| Website: www.y-str.org                    |");
            Console.WriteLine("| Developer: Felix Immanuel <i@fi.id.au>    |");
            Console.WriteLine("| Version: 1.4                              |");
            Console.WriteLine("| Build Date: 22-Jun-2015                   |");
            Console.WriteLine("+-------------------------------------------+");
        }

        public static void printSyntax()
        {
            Console.WriteLine("Syntax:");
            Console.WriteLine();
            Console.WriteLine("\taconv <in-file> <out-file> [options]");
            Console.WriteLine();
            Console.WriteLine("Optional Parameters:");
            Console.WriteLine(" -i  [Input Type]  - Value can be detect,ftdna,23andme,decodeme,ancestry or geno2. ");
            Console.WriteLine("                     This values is optional and not required. Use only if ");
            Console.WriteLine("                     autodetect fails. Default is detect");
            Console.WriteLine(" -o  [Output Type] - Value can be ftdna,23andme,ancestry,geno2, plink or eigenstrat.");
            Console.WriteLine("                     Default is ftdna");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            /* DEBUG 
            string file = @"C:\Users\chandraf\Downloads\NG58V56AGX.csv";
            string outfile = @"C:\Users\chandraf\Downloads\out.csv";
            int in_type = -1;
            int out_type = TYPE_FTDNA;
            */
            //args = new String[] { @"test.csv",@"test","-o","eigenstrat" };

            printLogo();
            Console.WriteLine();

            if (args.Length == 0)
            {
                printSyntax();
                return;
            }


            string file = @"C:\Users\chandraf\Downloads\Complete_Autosomal.csv";
            string outfile = @"C:\Users\chandraf\Downloads\Complete_Autosomal.txt";
            int in_type = -1;
            int out_type =  -1;


            if (args.Length < 2)
            {
                printSyntax();
                return;
            }
            else
            {
                try
                {
                    file = args[0];
                    outfile = args[1];

                    if(args.Length>2)
                    {

                        for (int i = 2; i < args.Length; i += 2)
                        {
                            if (args[i] == "-i")
                            {
                                if (args[i + 1].ToLower() == "detect")
                                    in_type = -1;
                                else if (args[i + 1].ToLower() == "ftdna")
                                    in_type = TYPE_FTDNA;
                                else if (args[i + 1].ToLower() == "23andme")
                                    in_type = TYPE_23ANDME;
                                else if (args[i + 1].ToLower() == "ancestry")
                                    in_type = TYPE_ANCESTRY;
                                else if (args[i + 1].ToLower() == "decodeme")
                                    in_type = TYPE_DECODEME;
                                else if (args[i + 1].ToLower() == "geno2")
                                    in_type = TYPE_GENO2;
                            }
                            if (args[i] == "-o")
                            {
                                if (args[i + 1].ToLower() == "ftdna")
                                    out_type = TYPE_FTDNA;
                                else if (args[i + 1].ToLower() == "23andme")
                                    out_type = TYPE_23ANDME;
                                else if (args[i + 1].ToLower() == "ancestry")
                                    out_type = TYPE_ANCESTRY;
                                else if (args[i + 1].ToLower() == "geno2")
                                    out_type = TYPE_GENO2;
                                else if (args[i + 1].ToLower() == "plink")
                                    out_type = TYPE_PLINK;
                                else if (args[i + 1].ToLower() == "eigenstrat")
                                    out_type = TYPE_EIGENSTRAT;
                                else
                                    out_type = TYPE_FTDNA;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error in Arguments.");
                    return;
                }
            }


            if (out_type == -1)
                out_type = TYPE_FTDNA;

            ArrayList rows = new ArrayList();

            in_type = updateMasterSNPlist(file, outfile, in_type, out_type);           
            if (in_type == TYPE_GENO2)
                Console.WriteLine("Warning: Geno 2.0 does not have any build positions in their file. Hence, output conversion from Geno 2.0 will not have any position details.");
            Console.WriteLine("Autosomal DNA file converted from " + TYPE[in_type] + " to " + TYPE[out_type] + " successfully.");

        }      
        
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private static int updateMasterSNPlist(string infile,string outfile, int intype, int outtype)
        {
            StreamWriter fw1 = null;
            StreamWriter fw2 = null;
            StreamWriter fw3 = null;

            if (outtype == TYPE_PLINK)
            {
                fw1 = new StreamWriter(outfile + ".ped");
                fw2 = new StreamWriter(outfile + ".map");
            }
            else if (outtype == TYPE_EIGENSTRAT)
            {
                fw1 = new StreamWriter(outfile + ".snp");
                fw2 = new StreamWriter(outfile + ".geno");
                fw3 = new StreamWriter(outfile + ".ind");
            }
            else
                fw1 = new StreamWriter(outfile);

            string allele1 = "";
            string allele2 = "";
            switch (outtype)
            {
                case TYPE_FTDNA:
                    fw1.WriteLine("RSID,CHROMOSOME,POSITION,RESULT");
                    break;
                case TYPE_23ANDME:
                    fw1.WriteLine("# rsid\tchromosome\tposition\tgenotype");
                    break;
                case TYPE_ANCESTRY:
                    fw1.WriteLine("rsid\tchromosome\tposition\tallele1\tallele2");
                    break;
                case TYPE_GENO2:
                    fw1.WriteLine("SNP,Chr,Allele1,Allele2");
                    break;
                case TYPE_PLINK:                    
                    fw1.Write("FAM1\t"+Path.GetFileNameWithoutExtension(infile)+"\t0\t0\t0\t0");
                    break;
                case TYPE_EIGENSTRAT:
                    fw3.WriteLine(Path.GetFileNameWithoutExtension(infile).Substring(0,(Path.GetFileNameWithoutExtension(infile).Length > 19 ? 19 : Path.GetFileNameWithoutExtension(infile).Length)).PadLeft(20) + " U" + "POPGRP".PadLeft(11));
                    break;
                default:
                    break;
            }

            //string[] lines = null;
            IEnumerable<string> lines = null;
            FileInfo fi = new FileInfo(infile);
            bool gz = false;
            if (infile.EndsWith(".gz"))
            {
                infile = Decompress(fi);
                gz = true;
            }
            
            lines = File.ReadLines(infile); //File.ReadAllLines(file);

            if(intype == -1)
                intype = detectDNAFileType(lines);
            
            if (intype == -1)
            {
                Console.WriteLine("Unable to identify file format for "+infile);
                Environment.Exit(0);
            }
            string[] data = null;
            string tLine=null;
            string rsid = null;
            string chr = null;
            string pos = null;
            string genotype = null;
            foreach (string line in lines)
            {
                //
                if (intype == TYPE_FTDNA)
                {
                    if (line.StartsWith("RSID"))
                        continue;
                    if (line.Trim() == "")
                        continue;
                    //
                    tLine = line.Replace("\"", "");
                    data = tLine.Split(",".ToCharArray());
                    rsid = data[0];
                    chr = data[1];
                    pos = data[2];
                    genotype = data[3];
                }
                if (intype == TYPE_23ANDME)
                {
                    if (line.StartsWith("#"))
                        continue;
                    if (line.Trim() == "")
                        continue;
                    //       
                    data = line.Split("\t".ToCharArray());
                    rsid = data[0];
                    chr = data[1];
                    pos = data[2];
                    genotype = data[3];
                }
                if (intype == TYPE_ANCESTRY)
                {
                    if (line.StartsWith("#"))
                        continue;
                    if (line.StartsWith("rsid\t"))
                        continue;
                    if (line.Trim() == "")
                        continue;
                    //            
                    data = line.Split("\t".ToCharArray());   

                    rsid = data[0];
                    chr = data[1];
                    if (chr == "23")
                        chr = "X";
                    pos = data[2];
                    genotype = data[3] + data[4];
                }
                if (intype == TYPE_GENO2)
                {
                    if (line.StartsWith("SNP,"))
                        continue;
                    if (line.Trim() == "")
                        continue;
                    //            
                    data = line.Split(",".ToCharArray());

                    rsid = data[0];
                    chr = data[1];
                    pos = getPosition(rsid);
                    genotype = data[2] + data[3];
                }
                if (intype == TYPE_DECODEME)
                {
                    if (line.StartsWith("Name,"))
                        continue;
                    if (line.Trim() == "")
                        continue;
                    //            
                    data = line.Split(",".ToCharArray());

                    rsid = data[0];
                    chr = data[2];                    
                    pos = data[3];
                    genotype = data[5];
                }
                //rows.Add(new string[]{rsid,chr,pos,genotype});

                if (outtype == TYPE_FTDNA)
                {
                    if (chr == "Y" || chr == "M" || chr == "MT" || chr == "24" || chr == "25")
                        continue;

                    if (chr == "23")
                        chr = "X";
                    else if (chr == "24")
                        chr = "Y";
                    fw1.WriteLine("\"" + rsid + "\",\"" + chr + "\",\"" + pos + "\",\"" + genotype.Replace("0", "-") + "\"");
                }
                else if (outtype == TYPE_23ANDME)
                {
                    if (chr == "23")
                        chr = "X";
                    else if (chr == "24")
                        chr = "Y";
                    else if (chr == "25" || chr == "M")
                        chr = "MT";
                    fw1.WriteLine(rsid + "\t" + chr + "\t" + pos + "\t" + genotype.Replace("0", "-"));
                }
                else if (outtype == TYPE_ANCESTRY)
                {
                    if (chr == "X")
                        chr = "23";
                    else if (chr == "Y")
                        chr = "24";
                    else if (chr == "MT" || chr == "M")
                        chr = "25";
                    allele1 = genotype[0].ToString();
                    allele2 = genotype[1].ToString();
                    if (allele1 == "-")
                        allele1 = "0";
                    if (allele2 == "-")
                        allele2 = "0";
                    fw1.WriteLine(rsid + "\t" + chr + "\t" + pos + "\t" + allele1 + "\t" + allele2);
                }
                else if (outtype == TYPE_GENO2)
                {
                    if (chr == "23")
                        chr = "X";
                    else if (chr == "24")
                        chr = "Y";
                    else if (chr == "25" || chr == "M")
                        chr = "Mt";
                    allele1 = genotype[0].ToString();
                    allele2 = genotype[1].ToString();
                    if (allele1 == "-")
                        allele1 = "0";
                    if (allele2 == "-")
                        allele2 = "0";
                    fw1.WriteLine(rsid + "," + chr + "," + allele1 + "," + allele2);
                }
                else if (outtype == TYPE_PLINK)
                {
                    if (chr == "23")
                        chr = "X";
                    else if (chr == "24")
                        chr = "Y";
                    else if (chr == "25" || chr == "M")
                        chr = "MT";
                    if (genotype.Length == 2)
                    {
                        allele1 = genotype[0].ToString();
                        allele2 = genotype[1].ToString();
                        if (allele1 == "-")
                            allele1 = "0";
                        if (allele2 == "-")
                            allele2 = "0";
                        fw1.Write("\t" + allele1 + "\t" + allele2);
                        fw2.WriteLine(chr + "\t" + rsid + "\t0\t" + pos);
                    }
                    else if (genotype.Length == 1)                    
                    {
                        allele1 = genotype[0].ToString();
                        allele2 = genotype[0].ToString();
                        if (allele1 == "-")
                            allele1 = "0";
                        if (allele2 == "-")
                            allele2 = "0";
                        fw1.Write("\t" + allele1 + "\t" + allele2);
                        fw2.WriteLine(chr + "\t" + rsid + "\t0\t" + pos);
                    }
                   
                }
                else if (outtype == TYPE_EIGENSTRAT)
                {
                    if (chr == "X")
                        chr = "23";
                    else if (chr == "Y")
                        chr = "24";
                    else if (chr == "25" || chr == "M" || chr == "MT")
                        chr = "90";
                    if (genotype.Length == 1)
                    {
                        allele1 = genotype[0].ToString();
                        allele2 = "X";
                        if (allele1 == "-")
                            allele1 = "X";
                        if (allele1 != "X")
                        {
                            fw1.WriteLine(rsid.PadLeft(20) + chr.PadLeft(4) + "0.000000".PadLeft(16) + pos.PadLeft(16) + allele1.PadLeft(2) + allele2.PadLeft(2));
                            fw2.WriteLine("1");
                        }
                    }
                    else if (genotype.Length == 2)
                    {
                        allele1 = genotype[0].ToString();
                        allele2 = genotype[1].ToString();
                        if (allele1 == "-")
                            allele1 = "X";
                        if (allele2 == "-")
                            allele2 = "X";
                        fw1.WriteLine(rsid.PadLeft(20) + chr.PadLeft(4) + "0.000000".PadLeft(16) + pos.PadLeft(16) + allele1.PadLeft(2) + allele2.PadLeft(2));
                        fw2.WriteLine("2");
                    }
                        
                }
            }
            fw1.Close();
            if (fw2 != null)
                fw2.Close();
            if (fw3 != null)
                fw3.Close();
            if (gz)
                File.Delete(infile);
            return intype;
        }

        private static int detectDNAFileType(IEnumerable<string> lines)
        {
            int count = 0;
            foreach (string line in lines)
            {
                if (line == "RSID,CHROMOSOME,POSITION,RESULT")
                    return TYPE_FTDNA;
                if (line == "# rsid\tchromosome\tposition\tgenotype")
                    return TYPE_23ANDME;
                if (line == "rsid\tchromosome\tposition\tallele1\tallele2")
                    return TYPE_ANCESTRY;
                if (line == "Name,Variation,Chromosome,Position,Strand,YourCode")
                    return TYPE_DECODEME;
                if (line == "SNP,Chr,Allele1,Allele2")
                    return TYPE_GENO2;
                /* if above doesn't work */
                if (line.Split("\t".ToCharArray()).Length == 4)
                    return TYPE_23ANDME;
                if (line.Split("\t".ToCharArray()).Length == 5)
                    return TYPE_ANCESTRY;
                if (line.Split(",".ToCharArray()).Length == 4)
                    return TYPE_FTDNA;
                if (line.Split(",".ToCharArray()).Length == 6)
                    return TYPE_DECODEME;
                if (count > 100)
                {
                    // detection useless... 
                    break;
                }
                count++;
            }
            return -1;
        }

        public static string Decompress(FileInfo fileToDecompress)
        {
            string newFileName = null;
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
            return newFileName;
        }

        private static string getPosition(string rsid)
        {
            return "0";
        }
    }
}
