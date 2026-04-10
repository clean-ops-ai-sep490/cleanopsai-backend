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
    public class SlaTaskRepositoryTests
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

        private WorkArea CreateWorkArea()
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

        private SlaTask CreateSlaTask(Guid slaId)
        {
            return new SlaTask
            {
                Id = Guid.NewGuid(),
                SlaId = slaId,
                Name = "Cleaning Task",

                // ✅ FIX QUAN TRỌNG (tránh lỗi của m)
                RecurrenceType = "DAILY",
                RecurrenceConfig = "{ \"interval\": 1 }",

                IsDeleted = false
            };
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddSlaTask()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);
            await context.SaveChangesAsync();

            var repo = new SlaTaskRepository(context);

            var task = CreateSlaTask(sla.Id);

            var result = await repo.CreateAsync(task);

            Assert.Equal(1, result);
            Assert.Equal(1, context.Set<SlaTask>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnTask()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);
            var task = CreateSlaTask(sla.Id);

            context.AddRange(client, contract, workArea, sla, task);
            await context.SaveChangesAsync();

            var repo = new SlaTaskRepository(context);

            var result = await repo.GetByIdAsync(task.Id);

            Assert.NotNull(result);
            Assert.Equal(task.Id, result.Id);
            Assert.NotNull(result.Sla);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnTasks()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);

            context.Set<SlaTask>().AddRange(
                CreateSlaTask(sla.Id),
                CreateSlaTask(sla.Id)
            );

            await context.SaveChangesAsync();

            var repo = new SlaTaskRepository(context);

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedTasks()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);

            for (int i = 0; i < 5; i++)
            {
                context.Set<SlaTask>().Add(CreateSlaTask(sla.Id));
            }

            await context.SaveChangesAsync();

            var repo = new SlaTaskRepository(context);

            var (items, total) = await repo.GetAllPaginationAsync(1, 2);

            Assert.Equal(5, total);
            Assert.Equal(2, items.Count);
        }

        // ================================
        // GET BY SLA ID
        // ================================
        [Fact]
        public async Task GetBySlaIdAsync_ShouldReturnTasks()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);

            context.Set<SlaTask>().AddRange(
                CreateSlaTask(sla.Id),
                CreateSlaTask(sla.Id)
            );

            await context.SaveChangesAsync();

            var repo = new SlaTaskRepository(context);

            var result = await repo.GetBySlaIdAsync(sla.Id);

            Assert.Equal(2, result.Count);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateTask()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);
            var task = CreateSlaTask(sla.Id);

            context.AddRange(client, contract, workArea, sla, task);
            await context.SaveChangesAsync();

            var repo = new SlaTaskRepository(context);

            task.Name = "Updated Task";

            var result = await repo.UpdateAsync(task);

            Assert.Equal(1, result);

            var updated = await context.Set<SlaTask>().FindAsync(task.Id);
            Assert.Equal("Updated Task", updated.Name);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteTask()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);
            var task = CreateSlaTask(sla.Id);

            context.AddRange(client, contract, workArea, sla, task);
            await context.SaveChangesAsync();

            var repo = new SlaTaskRepository(context);

            var result = await repo.DeleteAsync(task.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<SlaTask>().FindAsync(task.Id);
            Assert.True(deleted.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn0_WhenNotFound()
        {
            var context = GetDbContext();
            var repo = new SlaTaskRepository(context);

            var result = await repo.DeleteAsync(Guid.NewGuid());

            Assert.Equal(0, result);
        }
    }
}
