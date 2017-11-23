using System;

namespace Ax.Serialization.QueryStringSerializer.ConsoleTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var serializer = new QueryStringSerializer(
                new QueryStringValueMetadataProvider());

            var albertEinstein =
                new Person
                {
                    FirstName = "Albert",
                    LastName = "Einstein",
                    BirthDate = new DateTime(1879, 3, 14),
                    Height = 1.75f
                };

            var queryString = serializer.Serialize(albertEinstein);

            Console.WriteLine(queryString);

            Console.ReadLine();
        }
    }
}
