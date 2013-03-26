using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestApplication.Controllers;
using System.Web.Mvc;

namespace TestApplication.Tests
{
    [TestClass]
    public class CalculatorControllerTest
    {
        [TestMethod]
        public void AddOneAndTwoTest()
        {
            //Arrange
            var controller = new CalculatorController();

            //Act
            var result = controller.Index(1, 2);

            //Assert
            Assert.AreEqual(3, GetResult(result));
        }

        [TestMethod]
        public void AddThreeAndFiveTest()
        {
            //Arrange
            var controller = new CalculatorController();

            //Act
            var result = controller.Index(3, 5);

            //Assert
            Assert.AreEqual(8, GetResult(result));
        }

        [TestMethod]
        public void AddMinusFiveAndTenTest()
        {
            //Arrange
            var controller = new CalculatorController();

            //Act
            var result = controller.Index(-5, 10);

            //Assert
            Assert.AreEqual(5, GetResult(result));
        }

        public int GetResult(ActionResult result)
        {
            Assert.IsInstanceOfType(result, typeof(ViewResult));

            return (int)((ViewResult)result).Model;
        }
    }
}
