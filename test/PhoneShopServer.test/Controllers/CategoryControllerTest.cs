using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneShopSharedLibrary.Models;
using PhoneShopSharedLibrary.Responses;
using PhoneShopServer.Controllers;
using PhoneShopServer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneShopServer.test.Controllers
{
    public class CategoryControllerTest
    {
        private readonly ICategory categoryInterface;
        private readonly CategoryController CategoryController;
        public CategoryControllerTest()
        {
            categoryInterface = A.Fake<ICategory>();

            // System Under Test  SUT
            CategoryController = new CategoryController(categoryInterface);
           
        }
        private static Category CreateFakeCategory() => A.Fake<Category>();
        [Fact]
        public void CategoryController_Create_ReturnCreated()
        {
            //Arrange
            var category = CreateFakeCategory();
            var serviceResponse = A.Fake<ServiceResponse>();

            //Act
            A.CallTo(()=> categoryInterface.AddCategory(category)).Returns(serviceResponse);
            var result =  CategoryController.AddCategory(category);

            // Assert
            result.Should().BeOfType<Task<ActionResult>>();           
            result.Should().NotBeNull();
        }
    }
}
