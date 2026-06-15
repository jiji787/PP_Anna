using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EfCoreDemo;

namespace EfCoreDemo.Tests.UnitTests
{
    public class UserRepositoryTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            using var context = GetInMemoryDbContext();
            context.Users.AddRange(
                new User { Id = 1, Name = "Яна", Age = 20, City = "Москва", Email = "yana@example.com" },
                new User { Id = 2, Name = "Иван", Age = 25, City = "СПб", Email = "ivan@example.com" }
            );
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Name == "Яна");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenIdExists()
        {
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { Id = 1, Name = "Яна", Age = 20, City = "Москва", Email = "yana@example.com" });
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Яна", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdNotFound()
        {
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);
            var result = await repository.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddUser()
        {
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);
            var newUser = new User { Name = "Новый", Age = 30, City = "Тверь", Email = "new@example.com" };

            await repository.AddAsync(newUser);
            var users = await context.Users.ToListAsync();

            Assert.Single(users);
            Assert.Equal("Новый", users[0].Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser()
        {
            using var context = GetInMemoryDbContext();
            var user = new User { Id = 1, Name = "Яна", Age = 20, City = "Москва", Email = "yana@example.com" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            user.Name = "Яна Обновленная";

            await repository.UpdateAsync(user);
            var updatedUser = await context.Users.FindAsync(1);

            Assert.NotNull(updatedUser);
            Assert.Equal("Яна Обновленная", updatedUser.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveUser_WhenIdExists()
        {
            using var context = GetInMemoryDbContext();
            context.Users.Add(new User { Id = 1, Name = "Яна", City = "Москва", Email = "yana@example.com" });
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);
            await repository.DeleteAsync(1);
            var users = await context.Users.ToListAsync();

            Assert.Empty(users);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_WhenIdNotFound()
        {
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);
            await repository.DeleteAsync(999);
            Assert.True(true);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowArgumentNullException_WhenUserIsNull()
        {
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null));
        }
    }
}