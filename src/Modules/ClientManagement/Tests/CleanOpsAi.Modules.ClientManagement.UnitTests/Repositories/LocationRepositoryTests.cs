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
    public class LocationRepositoryTests
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

        private Location CreateLocation(Guid clientId)
        {
            return new Location
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Name = "Location A",
                Address = "123 Street",
                Street = "Street 1",
                Commune = "Ward 1",
                Province = "HCM",
                IsDeleted = false
            };
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddLocation()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);
            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            var location = CreateLocation(client.Id);

            var result = await repo.CreateAsync(location);

            Assert.Equal(1, result);
            Assert.Equal(1, context.Set<Location>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnLocation()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            var location = CreateLocation(client.Id);
            context.Set<Location>().Add(location);

            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            var result = await repo.GetByIdAsync(location.Id);

            Assert.NotNull(result);
            Assert.Equal(location.Id, result.Id);
            Assert.NotNull(result.Client);
        }

        // ================================
        // GET ALL
        // ================================
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLocations()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            context.Set<Location>().AddRange(
                CreateLocation(client.Id),
                CreateLocation(client.Id)
            );

            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedLocations()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            for (int i = 0; i < 5; i++)
            {
                context.Set<Location>().Add(CreateLocation(client.Id));
            }

            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            var (items, totalCount) = await repo.GetAllPaginationAsync(1, 2);

            Assert.Equal(5, totalCount);
            Assert.Equal(2, items.Count);
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateLocation()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            var location = CreateLocation(client.Id);
            context.Set<Location>().Add(location);

            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            location.Name = "Updated Location";

            var result = await repo.UpdateAsync(location);

            Assert.Equal(1, result);

            var updated = await context.Set<Location>().FindAsync(location.Id);
            Assert.Equal("Updated Location", updated.Name);
        }

        // ================================
        // DELETE
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteLocation()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            var location = CreateLocation(client.Id);
            context.Set<Location>().Add(location);

            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            var result = await repo.DeleteAsync(location.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<Location>().FindAsync(location.Id);
            Assert.True(deleted.IsDeleted);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturn0_WhenNotFound()
        {
            var context = GetDbContext();
            var repo = new LocationRepository(context);

            var result = await repo.DeleteAsync(Guid.NewGuid());

            Assert.Equal(0, result);
        }

        // ================================
        // SOFT DELETE (METHOD RIÊNG)
        // ================================
        [Fact]
        public async Task SoftDeleteAsync_ShouldSoftDeleteLocation()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            var location = CreateLocation(client.Id);
            context.Set<Location>().Add(location);

            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            var result = await repo.SoftDeleteAsync(location.Id);

            Assert.Equal(1, result);

            var deleted = await context.Set<Location>().FindAsync(location.Id);
            Assert.True(deleted.IsDeleted);
        }

        // ================================
        // GET BY CLIENT ID
        // ================================
        [Fact]
        public async Task GetByClientIdAsync_ShouldReturnLocations()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            context.Set<Location>().AddRange(
                CreateLocation(client.Id),
                CreateLocation(client.Id)
            );

            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            var result = await repo.GetByClientIdAsync(client.Id);

            Assert.Equal(2, result.Count);
        }

        // ================================
        // GET BY CLIENT ID PAGINATION
        // ================================
        [Fact]
        public async Task GetByClientIdPaginationAsync_ShouldReturnPagedLocations()
        {
            var context = GetDbContext();
            var client = CreateClient();

            context.Set<Client>().Add(client);

            for (int i = 0; i < 5; i++)
            {
                context.Set<Location>().Add(CreateLocation(client.Id));
            }

            await context.SaveChangesAsync();

            var repo = new LocationRepository(context);

            var (items, totalCount) = await repo.GetByClientIdPaginationAsync(client.Id, 1, 3);

            Assert.Equal(5, totalCount);
            Assert.Equal(3, items.Count);
        }
    }
}
