using BethanysPieShop.Models;
using System;
using Xunit;

namespace BethanysPieShop.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void CanUpdatePiePrice()
        {
            // Arrange
            var pie = new Pie { Name = "Sample pie", Price = 12 };
            // Act
            pie.Price = 20;
            //Assert
            Assert.Equal(20, pie.Price);
        }

        [Fact]
        public void CanUpdatePieName()
        {
            // Arrange
            var pie = new Pie { Name = "Sample pie", Price = 12 };
            // Act
            pie.Name = "Another pie";
            //Assert
            Assert.Equal("Another pie", pie.Name);
        }
    }
}
