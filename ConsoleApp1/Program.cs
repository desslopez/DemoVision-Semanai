using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
   public class Program
    {
        public static void Main(string[] args)
        {
            string hello = "Hello world";
            Console.WriteLine(hello);


        }
        public string returnPath()
        {
            string folder = Environment.CurrentDirectory;
            return folder;
        }
    }
}
