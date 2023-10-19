namespace Unosquare.MemoryBlocks.Samples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sourceArray = new int[] { 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            using var block = NativeMemoryBlock.Allocate(sourceArray);

            //Console.WriteLine("Writing via addressing.");
            //for (var i = 0; i < sourceArray.Length; i++)
            //    block.Write(i * sizeof(int), i);

            Console.WriteLine("Reading via addressing.");
            for (var i = 0; i < sourceArray.Length; i++)
                Console.WriteLine(block.Read<int>(i * sizeof(int)));

            Console.WriteLine("Reading via streams.");
            using (var stream = block.ToStream(FileAccess.Read))
            {
                using var reader = new BinaryReader(stream);
                while (stream.Position < stream.Length)
                {
                    Console.WriteLine($"Stream Reder: {reader.ReadInt32()}");
                }
            }

            Console.WriteLine("Resize to half - 1 bytes (should be 4 ints and displaying 3.)");
            block.Resize((block.ByteLength / 2) - 1);

            Console.WriteLine("Reading via span iterations.");
            {
                var span = block.AsSpan<int>()[1..];
                foreach (var item in span)
                    Console.WriteLine(item);
            }


            Console.WriteLine("Resize to double + 1 bytes (should be 8 ints and displaying 7.)");
            block.Resize((block.ByteLength * 3) + 1);
            {
                var span = block.AsSpan<int>()[1..];
                foreach (var item in span)
                    Console.WriteLine(item);
            }
        }
    }
}