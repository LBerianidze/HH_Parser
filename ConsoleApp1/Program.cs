using System;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Test t = new Test();
            var b = t.datetime.HasValue;
        }
    }

    internal class Test
    {
        public DateTime? datetime;
    }
}
