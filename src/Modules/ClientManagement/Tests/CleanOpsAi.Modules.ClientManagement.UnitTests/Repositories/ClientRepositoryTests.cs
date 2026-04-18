using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Data;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.UnitTests.Repositories
{
    public class ClientRepositoryTests
    {
        private ClientManagementDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ClientManagementDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // mỗi test 1 DB riêng
                .Options;

            return new ClientManagementDbContext(options);
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddClient()
        {
            var context = GetDbContext();
            var repo = new ClientRepository(context);

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "test@gmail.com",
                IsDeleted = false
            };

            var result = await repo.CreateAsync(client);

            Assert.Equal(1, result);
            Assert.Equal(1, context.Set<Client>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnClient()
        {
            var context = GetDbContext();

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "test@gmail.com",
                IsDeleted = false
            };

            context.Set<Client>().Add(client);
            await context.SaveChangesAsync();

            var repo = new ClientRepository(context);

            var result = await repo.GetByIdAsync(client.Id);

            Assert.NotNull(result);
            Assert.Equal(client.Id, result.Id);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllClients()
        {
            var context = GetDbContext();

            context.Set<Client>().AddRange(
                new Client { Id = Guid.NewGuid(), Name = "A", Email = "a@gmail.com" },
                new Client { Id = Guid.NewGuid(), Name = "B", Email = "b@gmail.com" }
            );

            await context.SaveChangesAsync();

            var repo = new ClientRepository(context);

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateClient()
        {
            var context = GetDbContext();

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Old",
                Email = "old@gmail.com"
            };

            context.Set<Client>().Add(client);
            await context.SaveChangesAsync();

            var repo = new ClientRepository(context);

            client.Name = "New";

            var result = await repo.UpdateAsync(client);

            Assert.Equal(1, result);

            var updated = await context.Set<Client>().FindAsync(client.Id);
            Assert.Equal("New", updated.Name);
        }

        // ================================
        // DELETE (SOFT DELETE)
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDelete()
        {
            var context = GetDbContext();

            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "test@gmail.com",
                IsDeleted = false
            };

            context.Set<Client>().Add(client);
            await context.SaveChangesAsync();

            var repo = new ClientRepository(context);

            var result = await repo.DeleteAsync(client.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<Client>().FindAsync(client.Id);
            Assert.True(deleted.IsDeleted);
        }
    }
}
