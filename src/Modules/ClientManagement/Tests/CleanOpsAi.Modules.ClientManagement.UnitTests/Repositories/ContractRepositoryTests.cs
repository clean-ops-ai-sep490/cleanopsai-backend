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
    public class ContractRepositoryTests
    {
        private ClientManagementDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ClientManagementDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging() // debug nếu lỗi
                .Options;

            return new ClientManagementDbContext(options);
        }

        // ================================
        // DATA SEED
        // ================================
        private Client CreateClient()
        {
            return new Client
            {
                Id = Guid.NewGuid(),
                Name = "Client A",
                Email = "client@gmail.com",
                IsDeleted = false
            };
        }

        private Contract CreateContract(Guid clientId)
        {
            return new Contract
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Name = "Contract A",
                UrlFile = "https://file.com/contract.pdf", // ✅ FIX QUAN TRỌNG
                IsDeleted = false
            };
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddContract()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);
            await context.SaveChangesAsync();

            var repo = new ContractRepository(context);

            var contract = CreateContract(client.Id);

            var result = await repo.CreateAsync(contract);

            Assert.Equal(1, result);
            Assert.Equal(1, context.Set<Contract>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnContract()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            var contract = CreateContract(client.Id);
            context.Set<Contract>().Add(contract);

            await context.SaveChangesAsync();

            var repo = new ContractRepository(context);

            var result = await repo.GetByIdAsync(contract.Id);

            Assert.NotNull(result);
            Assert.Equal(contract.Id, result.Id);
            Assert.NotNull(result.Client);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllContracts()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            context.Set<Contract>().AddRange(
                CreateContract(client.Id),
                CreateContract(client.Id)
            );

            await context.SaveChangesAsync();

            var repo = new ContractRepository(context);

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // GET ALL PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedContracts()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            for (int i = 0; i < 5; i++)
            {
                context.Set<Contract>().Add(CreateContract(client.Id));
            }

            await context.SaveChangesAsync();

            var repo = new ContractRepository(context);

            var (items, totalCount) = await repo.GetAllPaginationAsync(1, 2);

            Assert.Equal(5, totalCount);
            Assert.Equal(2, items.Count);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateContract()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            var contract = CreateContract(client.Id);
            context.Set<Contract>().Add(contract);

            await context.SaveChangesAsync();

            var repo = new ContractRepository(context);

            contract.Name = "Updated";

            var result = await repo.UpdateAsync(contract);

            Assert.Equal(1, result);

            var updated = await context.Set<Contract>().FindAsync(contract.Id);
            Assert.Equal("Updated", updated.Name);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteContract()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            var contract = CreateContract(client.Id);
            context.Set<Contract>().Add(contract);

            await context.SaveChangesAsync();

            var repo = new ContractRepository(context);

            var result = await repo.DeleteAsync(contract.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<Contract>().FindAsync(contract.Id);
            Assert.True(deleted.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn0_WhenNotFound()
        {
            var context = GetDbContext();
            var repo = new ContractRepository(context);

            var result = await repo.DeleteAsync(Guid.NewGuid());

            Assert.Equal(0, result);
        }

        // ================================
        // GET BY CLIENT ID
        // ================================
        [Fact]
        public async Task GetByClientIdAsync_ShouldReturnContracts()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            context.Set<Contract>().AddRange(
                CreateContract(client.Id),
                CreateContract(client.Id)
            );

            await context.SaveChangesAsync();

            var repo = new ContractRepository(context);

            var result = await repo.GetByClientIdAsync(client.Id);

            Assert.Equal(2, result.Count);
        }

        // ================================
        // GET BY CLIENT ID PAGINATION
        // ================================
        [Fact]
        public async Task GetByClientIdPaginationAsync_ShouldReturnPagedContracts()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            for (int i = 0; i < 5; i++)
            {
                context.Set<Contract>().Add(CreateContract(client.Id));
            }

            await context.SaveChangesAsync();

            var repo = new ContractRepository(context);

            var (items, totalCount) = await repo.GetByClientIdPaginationAsync(client.Id, 1, 3);

            Assert.Equal(5, totalCount);
            Assert.Equal(3, items.Count);
        }
    }
}
