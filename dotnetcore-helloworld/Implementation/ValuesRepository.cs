using System.Collections.Generic;

namespace Implementation
{
    public class ValuesRepository
    {
        public IEnumerable<string> GetAll()
        {
            return new[] { "value1", "value2" };
        }

        public string GetById(int id)
        {
            return "value";
        }
    }
}
