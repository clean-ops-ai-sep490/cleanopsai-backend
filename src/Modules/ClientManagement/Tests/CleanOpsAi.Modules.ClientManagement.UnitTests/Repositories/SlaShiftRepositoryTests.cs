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
    public class SlaShiftRepositoryTests
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

        private SlaShift CreateSlaShift(Guid slaId)
        {
            return new SlaShift
            {
                Id = Guid.NewGuid(),
                SlaId = slaId,
                Name = "Morning Shift", // ✅ FIX QUAN TRỌNG
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(17, 0),
                IsDeleted = false
            };
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddSlaShift()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);
            await context.SaveChangesAsync();

            var repo = new SlaShiftRepository(context);

            var shift = CreateSlaShift(sla.Id);

            var result = await repo.CreateAsync(shift);

            Assert.Equal(1, result);
            Assert.Equal(1, context.Set<SlaShift>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnSlaShift()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);
            var shift = CreateSlaShift(sla.Id);

            context.AddRange(client, contract, workArea, sla, shift);
            await context.SaveChangesAsync();

            var repo = new SlaShiftRepository(context);

            var result = await repo.GetByIdAsync(shift.Id);

            Assert.NotNull(result);
            Assert.Equal(shift.Id, result.Id);
            Assert.NotNull(result.Sla);
        }

        // ================================
        // GET BY SLA ID
        // ================================
        [Fact]
        public async Task GetBySlaIdAsync_ShouldReturnShifts()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);

            context.Set<SlaShift>().AddRange(
                CreateSlaShift(sla.Id),
                CreateSlaShift(sla.Id)
            );

            await context.SaveChangesAsync();

            var repo = new SlaShiftRepository(context);

            var result = await repo.GetBySlaIdAsync(sla.Id);

            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedShifts()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);

            context.AddRange(client, contract, workArea, sla);

            for (int i = 0; i < 5; i++)
            {
                context.Set<SlaShift>().Add(CreateSlaShift(sla.Id));
            }

            await context.SaveChangesAsync();

            var repo = new SlaShiftRepository(context);

            var (items, total) = await repo.GetAllPaginationAsync(1, 2);

            Assert.Equal(5, total);
            Assert.Equal(2, items.Count);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateShift()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);
            var shift = CreateSlaShift(sla.Id);

            context.AddRange(client, contract, workArea, sla, shift);
            await context.SaveChangesAsync();

            var repo = new SlaShiftRepository(context);

            shift.Name = "Updated Shift";

            var result = await repo.UpdateAsync(shift);

            Assert.Equal(1, result);

            var updated = await context.Set<SlaShift>().FindAsync(shift.Id);
            Assert.Equal("Updated Shift", updated.Name);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteShift()
        {
            var context = GetDbContext();

            var client = CreateClient();
            var contract = CreateContract(client.Id);
            var workArea = CreateWorkArea();
            var sla = CreateSla(contract.Id, workArea.Id);
            var shift = CreateSlaShift(sla.Id);

            context.AddRange(client, contract, workArea, sla, shift);
            await context.SaveChangesAsync();

            var repo = new SlaShiftRepository(context);

            var result = await repo.DeleteAsync(shift.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<SlaShift>().FindAsync(shift.Id);
            Assert.True(deleted.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn0_WhenNotFound()
        {
            var context = GetDbContext();
            var repo = new SlaShiftRepository(context);

            var result = await repo.DeleteAsync(Guid.NewGuid());

            Assert.Equal(0, result);
        }
    }
}
