using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShopSharedLibrary.DTOs;
using PhoneShopSharedLibrary.Responses;
using PhoneShopServer.Data;
using PhoneShopServer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneShopServer.test.Repository
{
    public class AuthenticationReposotoryTest
    {
        private async Task<AppDbContext> GetDataBaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (!await databaseContext.ApplicationUser.AnyAsync())
            {
                for (int i = 1; i < 10; i++)
                {
                    databaseContext.ApplicationUser.Add(
                        new ApplicationUser { Name = $" User{i}", Email = $"User{i}@system.com", Id = i, Password= "Password@123" });
                }
                await databaseContext.SaveChangesAsync();
            }
            return databaseContext;
        }

        [Fact]
        public async void AuthenticationReposotory_Register_ReturnBoolean()
        {
            //Arrange
            var newUser = new UserDTO { Name = $" User11", Email = $"Email User11", Id = 11, Password = "Password@123", ConfirmPassword = "Password@123" };
            var dbContext = await GetDataBaseContext();
            var userRepo = new AuthenticationReposotory(dbContext);

            // Act
            var result = await userRepo.Register(newUser);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ServiceResponse>();
            result.flag.Should().BeTrue();
            result.message.Equals("User is successfullyy registered");

        }
    }
}
