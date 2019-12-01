using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct Rect{
    public UInt32 x1 { get; set; }
    public UInt32 y1 { get; set; }
    public UInt32 x2 { get; set; }
    public UInt32 y2 { get; set; }
}

namespace EMF_SPL_to_EMF
{
    class Program
    {
        public static int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0)
        {
            int found = -1;
            bool matched = false;
            //only look at this if we have a populated search array and search bytes with a sensible start
            if (searchIn.Length > 0 && searchBytes.Length > 0 && start <= (searchIn.Length - searchBytes.Length) && searchIn.Length >= searchBytes.Length)
            {
                //iterate through the array to be searched
                for (int i = start; i <= searchIn.Length - searchBytes.Length; i++)
                {
                    //if the start bytes match we will start comparing all other bytes
                    if (searchIn[i] == searchBytes[0])
                    {
                        if (searchIn.Length > 1)
                        {
                            //multiple bytes to be searched we have to compare byte by byte
                            matched = true;
                            for (int y = 1; y <= searchBytes.Length - 1; y++)
                            {
                                if (searchIn[i + y] != searchBytes[y])
                                {
                                    matched = false;
                                    break;
                                }
                            }
                            //everything matched up
                            if (matched)
                            {
                                found = i;
                                break;
                            }
                        }
                        else
                        {
                            //search byte is only one bit nothing else to do
                            found = i;
                            break; //stop the loop
                        }
                    }
                }
            }
            return found;
        }

        static int Main(string[] args)
        {
            string targetFile = "Test\\00036";
            string dataAll = "";
            int version, dataSP, fileNameSP, printPortSP, dataSize, searchSP = 0, emfTextSP = -1;
            byte[] file = File.ReadAllBytes(targetFile + ".SPL"), intByte = new byte[4], emfData,
                emrExtTextOutWType = { 84, 0, 0, 0 };
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

            while ((emfTextSP = ByteSearch(emfData, emrExtTextOutWType, searchSP)) != -1)
            {
                UInt32 emfTextSize = BitConverter.ToUInt32(emfData.Skip(emfTextSP + 4).Take(4).ToArray(), 0);
                UInt32 textLength = 0, i;
                UInt32 scaleX = 0, scaleY = 0;
                Rect textRect = new Rect();
                Console.WriteLine("EMF EXTTEXTOUTW SP: {0}, SIZE: {1}", emfTextSP, emfTextSize);

                if (BitConverter.ToInt32(emfData.Skip(emfTextSP + 24).Take(4).ToArray(), 0) == 1)
                {
                    scaleX = BitConverter.ToUInt32(emfData.Skip(emfTextSP + 28).Take(4).ToArray(), 0);
                    scaleY = BitConverter.ToUInt32(emfData.Skip(emfTextSP + 32).Take(4).ToArray(), 0);

                    textLength = BitConverter.ToUInt32(emfData.Skip(emfTextSP + 44).Take(4).ToArray(), 0) * 2;

                    Console.WriteLine("SCALE X: {0}, Y: {1}, LEN: {2}", scaleX, scaleY, textLength);

                    textRect.x1 = BitConverter.ToUInt32(emfData.Skip(emfTextSP + 56).Take(4).ToArray(), 0);
                    textRect.y1 = BitConverter.ToUInt32(emfData.Skip(emfTextSP + 60).Take(4).ToArray(), 0);
                    textRect.x2 = BitConverter.ToUInt32(emfData.Skip(emfTextSP + 64).Take(4).ToArray(), 0);
                    textRect.y2 = BitConverter.ToUInt32(emfData.Skip(emfTextSP + 68).Take(4).ToArray(), 0);

                    Console.WriteLine("RECT: {0},{1} {2},{3}", textRect.x1, textRect.y1, textRect.x2, textRect.y2);

                    string text = System.Text.Encoding.Unicode.GetString(emfData.Skip(emfTextSP + 76).Take((int)textLength).ToArray());

                    Console.WriteLine("TEXT: {0}", text);

                    dataAll += text;
                }
                else
                {
                    Console.WriteLine("No Disply mode.");
                }

                if ((int)(emfTextSP + emfTextSize) < 0)
                {
                    Console.WriteLine("Stack overflow");
                    break;
                }
                else
                {
                    searchSP = (int)(emfTextSP + (100 > emfTextSize ? emfTextSize : 100));
                }

                Console.WriteLine("Next SearchSP: {0}", searchSP);
            }

            Console.WriteLine("End of process\n{0}", dataAll);

            return 0;
        }
    }
}
