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
    public class ZoneRepositoryTests
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
        private Location CreateLocation()
        {
            return new Location
            {
                Id = Guid.NewGuid(),
                Name = "Location A",
                Address = "HCM",
                IsDeleted = false
            };
        }

        private Zone CreateZone(Guid locationId)
        {
            return new Zone
            {
                Id = Guid.NewGuid(),
                LocationId = locationId,

                // ⚠️ tránh lỗi required
                Name = "Zone A",

                IsDeleted = false
            };
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddZone()
        {
            var context = GetDbContext();

            var location = CreateLocation();
            context.Add(location);
            await context.SaveChangesAsync();

            var repo = new ZoneRepository(context);

            var zone = CreateZone(location.Id);

            var result = await repo.CreateAsync(zone);

            Assert.Equal(1, result);
            Assert.Equal(1, context.Set<Zone>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnZone()
        {
            var context = GetDbContext();

            var location = CreateLocation();
            var zone = CreateZone(location.Id);

            context.AddRange(location, zone);
            await context.SaveChangesAsync();

            var repo = new ZoneRepository(context);

            var result = await repo.GetByIdAsync(zone.Id);

            Assert.NotNull(result);
            Assert.Equal(zone.Id, result.Id);
            Assert.NotNull(result.Location);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnZones()
        {
            var context = GetDbContext();

            var location = CreateLocation();
            context.Add(location);

            context.Set<Zone>().AddRange(
                CreateZone(location.Id),
                CreateZone(location.Id)
            );

            await context.SaveChangesAsync();

            var repo = new ZoneRepository(context);

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedZones()
        {
            var context = GetDbContext();

            var location = CreateLocation();
            context.Add(location);

            for (int i = 0; i < 5; i++)
            {
                context.Set<Zone>().Add(CreateZone(location.Id));
            }

            await context.SaveChangesAsync();

            var repo = new ZoneRepository(context);

            var (items, total) = await repo.GetAllPaginationAsync(1, 2);

            Assert.Equal(5, total);
            Assert.Equal(2, items.Count);
        }

        // ================================
        // FILTER BY LOCATION
        // ================================
        [Fact]
        public async Task GetByLocationIdPaginationAsync_ShouldReturnFilteredZones()
        {
            var context = GetDbContext();

            var location1 = CreateLocation();
            var location2 = CreateLocation();

            context.AddRange(location1, location2);

            // location1 có 3
            for (int i = 0; i < 3; i++)
            {
                context.Set<Zone>().Add(CreateZone(location1.Id));
            }

            // location2 có 2
            for (int i = 0; i < 2; i++)
            {
                context.Set<Zone>().Add(CreateZone(location2.Id));
            }

            await context.SaveChangesAsync();

            var repo = new ZoneRepository(context);

            var (items, total) = await repo.GetByLocationIdPaginationAsync(location1.Id, 1, 10);

            Assert.Equal(3, total);
            Assert.All(items, x => Assert.Equal(location1.Id, x.LocationId));
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateZone()
        {
            var context = GetDbContext();

            var location = CreateLocation();
            var zone = CreateZone(location.Id);

            context.AddRange(location, zone);
            await context.SaveChangesAsync();

            var repo = new ZoneRepository(context);

            zone.Name = "Updated Zone";

            var result = await repo.UpdateAsync(zone);

            Assert.Equal(1, result);

            var updated = await context.Set<Zone>().FindAsync(zone.Id);
            Assert.Equal("Updated Zone", updated.Name);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteZone()
        {
            var context = GetDbContext();

            var location = CreateLocation();
            var zone = CreateZone(location.Id);

            context.AddRange(location, zone);
            await context.SaveChangesAsync();

            var repo = new ZoneRepository(context);

            var result = await repo.DeleteAsync(zone.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<Zone>().FindAsync(zone.Id);
            Assert.True(deleted.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn0_WhenNotFound()
        {
            var context = GetDbContext();
            var repo = new ZoneRepository(context);

            var result = await repo.DeleteAsync(Guid.NewGuid());

            Assert.Equal(0, result);
        }
    }
}
