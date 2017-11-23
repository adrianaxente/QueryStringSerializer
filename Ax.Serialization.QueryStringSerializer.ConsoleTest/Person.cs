using System;
namespace Ax.Serialization.QueryStringSerializer.ConsoleTest
{
    public class Person
    {
        [QueryStringValue("name.first")]
        public string FirstName { get; set; }

        [QueryStringValue("name.last")]
        public string LastName { get; set; }

        [QueryStringValue("birthDate")]
        public DateTime BirthDate { get; set; }

        public float Height { get; set; }

    }
}
