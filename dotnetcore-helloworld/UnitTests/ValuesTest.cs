using System.Linq;
using Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ValuesTest
    {
        [TestMethod]
        public void ReturnsAllValues()
        {
            var repository = new ValuesRepository();

            var values = repository.GetAll();

            Assert.AreEqual(2, values.Count());
        }

        [TestMethod]
        public void ReturnsSpecificValues()
        {
            var repository = new ValuesRepository();

            var value = repository.GetById(0);

            Assert.AreEqual("value", value);
        }
    }
}
