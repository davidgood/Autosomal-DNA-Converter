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

        public static string[] TYPE = { "ftdna", "23andme", "ancestry","decodeme","geno2"};

        public static void printLogo()
        {
            Console.WriteLine("+----------------------------------+");
            Console.WriteLine("| Product: Autosomal DNA Converter |");
            Console.WriteLine("| Website: www.y-str.org           |");
            Console.WriteLine("| Developer: Felix <i@fc.id.au>    |");
            Console.WriteLine("| Version: 1.2                     |");
            Console.WriteLine("| Build Date: 29-Sep-2013          |");
            Console.WriteLine("+----------------------------------+");
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
            Console.WriteLine(" -o  [Output Type] - Value can be ftdna,23andme,ancestry or geno2. Default is ftdna");
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
            //args = new String[] { @"C:\Users\chandraf\Downloads\NG58V56AGX.csv",
            //                       @"C:\Users\chandraf\Downloads\out.csv" };

            printLogo();
            Console.WriteLine();
            if (args.Length == 0)
            {
                printSyntax();
                return;
            }

            string file = @"C:\Users\chandraf\Downloads\NG58V56AGX.csv";
            string outfile = null;
            int in_type = -1;
            int out_type = -1;

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

            in_type=updateMasterSNPlist(file, rows, in_type);
            convertFileSNPlist(outfile, rows, out_type);
            if (in_type == TYPE_GENO2)
                Console.WriteLine("Warning: Geno 2.0 does not have any build positions in their file. Hence, output conversion from Geno 2.0 will not have any position details.");
            Console.WriteLine("Autosomal DNA file converted from " + TYPE[in_type] + " to " + TYPE[out_type] + " successfully.");
        }

        private static void convertFileSNPlist(string file,  ArrayList rows, int type)
        {
            StreamWriter fw = new StreamWriter(file);
            String chr = "";

            switch (type)
            {
                case TYPE_FTDNA:
                    fw.WriteLine("RSID,CHROMOSOME,POSITION,RESULT");
                    break;
                case TYPE_23ANDME:
                    fw.WriteLine("# rsid\tchromosome\tposition\tgenotype");
                    break;
                case TYPE_ANCESTRY:
                    fw.WriteLine("rsid\tchromosome\tposition\tallele1\tallele2");
                    break;
                case TYPE_GENO2:
                    fw.WriteLine("SNP,Chr,Allele1,Allele2");
                    break;
                default:
                    break;
            }
            string allele1 = "";
            string allele2 = "";
            string snp="";
            foreach (string[] data in rows)
            {
                chr = data[1];
                //
                if (type == TYPE_FTDNA)
                {
                    if (chr == "Y" || chr == "M" || chr == "MT" || chr == "24" || chr == "25")
                        continue;

                    if (chr == "23")
                        chr = "X";
                    else if (chr == "24")
                        chr = "Y";
                    snp = data[3];
                    snp=snp.Replace("0", "-");
                    fw.WriteLine("\"" + data[0] + "\",\"" + chr + "\",\"" + data[2] + "\",\"" + snp + "\"");
                }
                else if (type == TYPE_23ANDME)
                {
                    if (chr == "23")
                        chr = "X";
                    else if (chr == "24")
                        chr = "Y";
                    else if (chr == "25" || chr == "M")
                        chr = "MT";
                    snp = data[3];
                    snp = snp.Replace("0", "-");
                    fw.WriteLine(data[0] + "\t" + chr + "\t" + data[2] + "\t" + snp);
                }
                else if (type == TYPE_ANCESTRY)
                {                    
                    if (chr == "X")
                        chr = "23";
                    else if (chr == "Y")
                        chr = "24";
                    else if (chr == "MT" || chr == "M")
                        chr = "25";
                    allele1 = data[3][0].ToString();
                    allele2 = data[3][1].ToString();
                    if (allele1 == "-")
                        allele1 = "0";
                    if (allele2 == "-")
                        allele2 = "0";
                    fw.WriteLine(data[0] + "\t" + chr + "\t" + data[2] + "\t" + allele1 + "\t" + allele2);                    
                }
                else if (type == TYPE_GENO2)
                {
                    if (chr == "23")
                        chr = "X";
                    else if (chr == "24")
                        chr = "Y";
                    else if (chr == "25" || chr == "M")
                        chr = "Mt";
                    allele1 = data[3][0].ToString();
                    allele2 = data[3][1].ToString();
                    if (allele1 == "-")
                        allele1 = "0";
                    if (allele2 == "-")
                        allele2 = "0";
                    fw.WriteLine(data[0] + "," + chr + "," + allele1 + "," + allele2);
                }
            }
            fw.Close();
        }

        
        
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private static int updateMasterSNPlist(string file, ArrayList rows, int type)
        {
            string[] lines = null;
            string tmp = null;
            if (file.EndsWith(".gz"))
            {
                tmp=Encoding.UTF8.GetString(Unzip(File.ReadAllBytes(file)));
                tmp=tmp.Replace("\r\n","\r");
                lines = tmp.Split("\r".ToCharArray());
            }
            else
                lines = File.ReadAllLines(file);

            if(type == -1)
                type = detectDNAFileType(lines);
            
            if (type == -1)
            {
                Console.WriteLine("Unable to identify file format for "+file);
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
                if (type == TYPE_FTDNA)
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
                if (type == TYPE_23ANDME)
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
                if (type == TYPE_ANCESTRY)
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
                if (type == TYPE_GENO2)
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
                if (type == TYPE_DECODEME)
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
                rows.Add(new string[]{rsid,chr,pos,genotype});
            }
            return type;
        }

        private static int detectDNAFileType(string[] lines)
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

        public static byte[] Zip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static byte[] Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        private static string getPosition(string rsid)
        {
            return "0";
        }
    }
}
