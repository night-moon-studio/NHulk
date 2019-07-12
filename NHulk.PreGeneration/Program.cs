using NHulk.Connection;
using System;

namespace NHulk.PreGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = SqlConfig.GetConnectionString("XXSystem");
            //Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
