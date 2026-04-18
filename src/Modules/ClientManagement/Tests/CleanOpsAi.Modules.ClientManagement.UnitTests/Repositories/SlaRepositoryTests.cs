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
    public class SlaRepositoryTests
    {
        private ClientManagementDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ClientManagementDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
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
                UrlFile = "test.pdf",
                IsDeleted = false
            };
        }

        private WorkArea CreateWorkArea(Guid clientId)
        {
            return new WorkArea
            {
                Id = Guid.NewGuid(),
                Name = "WorkArea A",
                IsDeleted = false
            };
        }

        private Sla CreateSla(Guid contractId, Guid workAreaId)
        {
            return new Sla
            {
                Id = Guid.NewGuid(),
                ContractId = contractId,
                WorkAreaId = workAreaId,
                Name = "SLA A",
                IsDeleted = false
            };
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddSla()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea(client.Id);

            context.AddRange(client, contract, workArea);
            await context.SaveChangesAsync();

            var repo = new SlaRepository(context);

            var sla = CreateSla(contract.Id, workArea.Id);

            var result = await repo.CreateAsync(sla);

            Assert.Equal(1, result);
            Assert.Equal(1, context.Set<Sla>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnSla()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea(client.Id);
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);
            await context.SaveChangesAsync();

            var repo = new SlaRepository(context);

            var result = await repo.GetByIdAsync(sla.Id);

            Assert.NotNull(result);
            Assert.Equal(sla.Id, result.Id);
            Assert.NotNull(result.Contract);
            Assert.NotNull(result.WorkArea);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSla()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea(client.Id);

            context.AddRange(client, contract, workArea);

            context.Set<Sla>().AddRange(
                CreateSla(contract.Id, workArea.Id),
                CreateSla(contract.Id, workArea.Id)
            );

            await context.SaveChangesAsync();

            var repo = new SlaRepository(context);

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedSla()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea(client.Id);

            context.AddRange(client, contract, workArea);

            for (int i = 0; i < 5; i++)
            {
                context.Set<Sla>().Add(CreateSla(contract.Id, workArea.Id));
            }

            await context.SaveChangesAsync();

            var repo = new SlaRepository(context);

            var (items, totalCount) = await repo.GetAllPaginationAsync(1, 2);

            Assert.Equal(5, totalCount);
            Assert.Equal(2, items.Count);
        }

        // ================================
        // FILTER PAGINATION
        // ================================
        [Fact]
        public async Task FilterPaginationAsync_ShouldFilterByContractId()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract1 = CreateContract(client.Id);
            var contract2 = CreateContract(client.Id);
            var workArea = CreateWorkArea(client.Id);

            context.AddRange(client, contract1, contract2, workArea);

            context.Set<Sla>().AddRange(
                CreateSla(contract1.Id, workArea.Id),
                CreateSla(contract2.Id, workArea.Id)
            );

            await context.SaveChangesAsync();

            var repo = new SlaRepository(context);

            var (items, totalCount) = await repo.FilterPaginationAsync(
                null,
                contract1.Id,
                1,
                10
            );

            Assert.Single(items);
            Assert.Equal(1, totalCount);
        }

        [Fact]
        public async Task FilterPaginationAsync_ShouldFilterByWorkAreaId()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea1 = CreateWorkArea(client.Id);
            var workArea2 = CreateWorkArea(client.Id);

            context.AddRange(client, contract, workArea1, workArea2);

            context.Set<Sla>().AddRange(
                CreateSla(contract.Id, workArea1.Id),
                CreateSla(contract.Id, workArea2.Id)
            );

            await context.SaveChangesAsync();

            var repo = new SlaRepository(context);

            var (items, totalCount) = await repo.FilterPaginationAsync(
                workArea1.Id,
                null,
                1,
                10
            );

            Assert.Single(items);
            Assert.Equal(1, totalCount);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateSla()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea(client.Id);
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);
            await context.SaveChangesAsync();

            var repo = new SlaRepository(context);

            sla.Name = "Updated SLA";

            var result = await repo.UpdateAsync(sla);

            Assert.Equal(1, result);

            var updated = await context.Set<Sla>().FindAsync(sla.Id);
            Assert.Equal("Updated SLA", updated.Name);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteSla()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea(client.Id);
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);
            await context.SaveChangesAsync();

            var repo = new SlaRepository(context);

            var result = await repo.DeleteAsync(sla.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<Sla>().FindAsync(sla.Id);
            Assert.True(deleted.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn0_WhenNotFound()
        {
            var context = GetDbContext();
            var repo = new SlaRepository(context);

            var result = await repo.DeleteAsync(Guid.NewGuid());

            Assert.Equal(0, result);
        }
    }
}
