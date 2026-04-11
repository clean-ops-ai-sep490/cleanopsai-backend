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
    public class WorkAreaRepositoryTests
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
        private Zone CreateZone()
        {
            return new Zone
            {
                Id = Guid.NewGuid(),
                Name = "Zone A",
                IsDeleted = false
            };
        }

        private WorkArea CreateWorkArea(Guid zoneId)
        {
            return new WorkArea
            {
                Id = Guid.NewGuid(),
                ZoneId = zoneId,

                // ⚠️ tránh lỗi required
                Name = "WorkArea A",

                IsDeleted = false
            };
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddWorkArea()
        {
            var context = GetDbContext();

            var zone = CreateZone();
            context.Add(zone);
            await context.SaveChangesAsync();

            var repo = new WorkAreaRepository(context);

            var workArea = CreateWorkArea(zone.Id);

            var result = await repo.CreateAsync(workArea);

            Assert.Equal(1, result);
            Assert.Equal(1, context.Set<WorkArea>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnWorkArea()
        {
            var context = GetDbContext();

            var zone = CreateZone();
            var workArea = CreateWorkArea(zone.Id);

            context.AddRange(zone, workArea);
            await context.SaveChangesAsync();

            var repo = new WorkAreaRepository(context);

            var result = await repo.GetByIdAsync(workArea.Id);

            Assert.NotNull(result);
            Assert.Equal(workArea.Id, result.Id);
            Assert.NotNull(result.Zone);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnWorkAreas()
        {
            var context = GetDbContext();

            var zone = CreateZone();

            context.Add(zone);

            context.Set<WorkArea>().AddRange(
                CreateWorkArea(zone.Id),
                CreateWorkArea(zone.Id)
            );

            await context.SaveChangesAsync();

            var repo = new WorkAreaRepository(context);

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedData()
        {
            var context = GetDbContext();

            var zone = CreateZone();
            context.Add(zone);

            for (int i = 0; i < 5; i++)
            {
                context.Set<WorkArea>().Add(CreateWorkArea(zone.Id));
            }

            await context.SaveChangesAsync();

            var repo = new WorkAreaRepository(context);

            var (items, total) = await repo.GetAllPaginationAsync(1, 2);

            Assert.Equal(5, total);
            Assert.Equal(2, items.Count);
        }

        // ================================
        // FILTER BY ZONE
        // ================================
        [Fact]
        public async Task GetByZoneIdPaginationAsync_ShouldReturnFilteredData()
        {
            var context = GetDbContext();

            var zone1 = CreateZone();
            var zone2 = CreateZone();

            context.AddRange(zone1, zone2);

            // zone1 có 3
            for (int i = 0; i < 3; i++)
            {
                context.Set<WorkArea>().Add(CreateWorkArea(zone1.Id));
            }

            // zone2 có 2
            for (int i = 0; i < 2; i++)
            {
                context.Set<WorkArea>().Add(CreateWorkArea(zone2.Id));
            }

            await context.SaveChangesAsync();

            var repo = new WorkAreaRepository(context);

            var (items, total) = await repo.GetByZoneIdPaginationAsync(zone1.Id, 1, 10);

            Assert.Equal(3, total);
            Assert.All(items, x => Assert.Equal(zone1.Id, x.ZoneId));
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateWorkArea()
        {
            var context = GetDbContext();

            var zone = CreateZone();
            var workArea = CreateWorkArea(zone.Id);

            context.AddRange(zone, workArea);
            await context.SaveChangesAsync();

            var repo = new WorkAreaRepository(context);

            workArea.Name = "Updated Name";

            var result = await repo.UpdateAsync(workArea);

            Assert.Equal(1, result);

            var updated = await context.Set<WorkArea>().FindAsync(workArea.Id);
            Assert.Equal("Updated Name", updated.Name);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDelete()
        {
            var context = GetDbContext();

            var zone = CreateZone();
            var workArea = CreateWorkArea(zone.Id);

            context.AddRange(zone, workArea);
            await context.SaveChangesAsync();

            var repo = new WorkAreaRepository(context);

            var result = await repo.DeleteAsync(workArea.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<WorkArea>().FindAsync(workArea.Id);
            Assert.True(deleted.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn0_WhenNotFound()
        {
            var context = GetDbContext();
            var repo = new WorkAreaRepository(context);

            var result = await repo.DeleteAsync(Guid.NewGuid());

            Assert.Equal(0, result);
        }
    }
}
