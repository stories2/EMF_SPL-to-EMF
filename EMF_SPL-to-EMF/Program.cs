using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMF_SPL_to_EMF
{
    class Program
    {
        static int Main(string[] args)
        {
            string targetFile = "Test\\00035";
            int version, dataSP, fileNameSP, printPortSP, dataSize;
            byte[] file = File.ReadAllBytes(targetFile + ".SPL"), intByte = new byte[4], emfData;
            Console.WriteLine("File loaded size: {0}", file.Length);

            version = BitConverter.ToInt32(file.Take(4).ToArray(), 0);
            dataSP = BitConverter.ToInt32(file.Skip(4).Take(4).ToArray(), 0);
            fileNameSP = BitConverter.ToInt32(file.Skip(8).Take(4).ToArray(), 0);
            printPortSP = BitConverter.ToInt32(file.Skip(12).Take(4).ToArray(), 0);

            Console.WriteLine("SPL VER: {0}, EMF DATA SP: {1}, FILE NAME SP: {2}, PRINT PORT SP: {3}", version, dataSP, fileNameSP, printPortSP);

            dataSize = BitConverter.ToInt32(file.Skip(dataSP + 4).Take(4).ToArray(), 0);

            Console.WriteLine("EMF DATA SIZE: {0}", dataSize);

            emfData = file.Skip(dataSP + 8).Take(dataSize).ToArray();

            File.WriteAllBytes(targetFile + ".emf", emfData);

            Console.WriteLine("End of process");

            return 0;
        }
    }
}
