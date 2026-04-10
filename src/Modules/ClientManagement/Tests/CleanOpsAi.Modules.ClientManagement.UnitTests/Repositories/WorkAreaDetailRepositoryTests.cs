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
    public class WorkAreaDetailRepositoryTests
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
        private WorkArea CreateWorkArea()
        {
            return new WorkArea
            {
                Id = Guid.NewGuid(),
                Name = "WorkArea A",
                IsDeleted = false
            };
        }

        private WorkAreaDetail CreateWorkAreaDetail(Guid workAreaId)
        {
            return new WorkAreaDetail
            {
                Id = Guid.NewGuid(),
                WorkAreaId = workAreaId,

                // ⚠️ tránh lỗi required
                Name = "Detail A",

                Created = DateTime.UtcNow,

                IsDeleted = false
            };
        }

        // ================================
        // CREATE
        // ================================
        [Fact]
        public async Task CreateAsync_ShouldAddWorkAreaDetail()
        {
            var context = GetDbContext();

            var workArea = CreateWorkArea();
            context.Add(workArea);
            await context.SaveChangesAsync();

            var repo = new WorkAreaDetailRepository(context);

            var entity = CreateWorkAreaDetail(workArea.Id);

            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(1, context.Set<WorkAreaDetail>().Count());
        }

        // ================================
        // GET BY ID
        // ================================
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity()
        {
            var context = GetDbContext();

            var workArea = CreateWorkArea();
            var detail = CreateWorkAreaDetail(workArea.Id);

            context.AddRange(workArea, detail);
            await context.SaveChangesAsync();

            var repo = new WorkAreaDetailRepository(context);

            var result = await repo.GetByIdAsync(detail.Id);

            Assert.NotNull(result);
            Assert.Equal(detail.Id, result.Id);
            Assert.NotNull(result.WorkArea);
        }

        // ================================
        // PAGINATION
        // ================================
        [Fact]
        public async Task GetAllPaginationAsync_ShouldReturnPagedData()
        {
            var context = GetDbContext();

            var workArea = CreateWorkArea();
            context.Add(workArea);

            for (int i = 0; i < 5; i++)
            {
                context.Set<WorkAreaDetail>().Add(CreateWorkAreaDetail(workArea.Id));
            }

            await context.SaveChangesAsync();

            var repo = new WorkAreaDetailRepository(context);

            var (items, total) = await repo.GetAllPaginationAsync(1, 2);

            Assert.Equal(5, total);
            Assert.Equal(2, items.Count);
        }

        // ================================
        // GET BY WORKAREA ID PAGINATION
        // ================================
        [Fact]
        public async Task GetByWorkAreaIdPaginationAsync_ShouldReturnFilteredData()
        {
            var context = GetDbContext();

            var workArea1 = CreateWorkArea();
            var workArea2 = CreateWorkArea();

            context.AddRange(workArea1, workArea2);

            // workArea1 có 3 record
            for (int i = 0; i < 3; i++)
            {
                context.Set<WorkAreaDetail>().Add(CreateWorkAreaDetail(workArea1.Id));
            }

            // workArea2 có 2 record
            for (int i = 0; i < 2; i++)
            {
                context.Set<WorkAreaDetail>().Add(CreateWorkAreaDetail(workArea2.Id));
            }

            await context.SaveChangesAsync();

            var repo = new WorkAreaDetailRepository(context);

            var (items, total) = await repo.GetByWorkAreaIdPaginationAsync(workArea1.Id, 1, 10);

            Assert.Equal(3, total);
            Assert.All(items, x => Assert.Equal(workArea1.Id, x.WorkAreaId));
        }

        // ================================
        // UPDATE
        // ================================
        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntity()
        {
            var context = GetDbContext();

            var workArea = CreateWorkArea();
            var detail = CreateWorkAreaDetail(workArea.Id);

            context.AddRange(workArea, detail);
            await context.SaveChangesAsync();

            var repo = new WorkAreaDetailRepository(context);

            detail.Name = "Updated Name";

            var result = await repo.UpdateAsync(detail);

            Assert.Equal(1, result);

            var updated = await context.Set<WorkAreaDetail>().FindAsync(detail.Id);
            Assert.Equal("Updated Name", updated.Name);
        }

        // ================================
        // DELETE (SOFT DELETE)
        // ================================
        [Fact]
        public async Task DeleteAsync_ShouldSoftDelete()
        {
            var context = GetDbContext();

            var workArea = CreateWorkArea();
            var detail = CreateWorkAreaDetail(workArea.Id);

            context.AddRange(workArea, detail);
            await context.SaveChangesAsync();

            var repo = new WorkAreaDetailRepository(context);

            var result = await repo.DeleteAsync(detail);

            Assert.Equal(1, result);

            var deleted = await context.Set<WorkAreaDetail>().FindAsync(detail.Id);
            Assert.True(deleted.IsDeleted);
        }
    }
}
