using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly ClientManagementDbContext _dbContext;
        public ContractRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // get Constract by id
        public async Task<Contract?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<Contract>()
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        // get all Contracts
        public async Task<List<Contract>> GetAllAsync()
        {
            return await _dbContext.Set<Contract>()
                .Include(c => c.Client)
                .OrderByDescending(c => c.Created)
                .ToListAsync();
        }

        // get all Contracts with pagination
        public async Task<(List<Contract> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Contract>().Include(c => c.Client).AsQueryable().Where(c => c.IsDeleted == false).OrderByDescending(c => c.Created);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // add Contract
        public async Task<int> CreateAsync(Contract contract)
        {
            _dbContext.Set<Contract>().Add(contract);
            return await _dbContext.SaveChangesAsync();
        }

        // update Contract
        public async Task<int> UpdateAsync(Contract contract)
        {
            _dbContext.Set<Contract>().Update(contract);
            return await _dbContext.SaveChangesAsync();
        }

        // Soft delete contract
        public async Task<int> DeleteAsync(Guid id)
        {
            var contract = await _dbContext.Set<Contract>()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return 0;
            }

            contract.IsDeleted = true;

            _dbContext.Set<Contract>().Update(contract);

            return await _dbContext.SaveChangesAsync();
        }

        // get contracts by clientId
        public async Task<List<Contract>> GetByClientIdAsync(Guid clientId)
        {
            return await _dbContext.Set<Contract>()
                .Include(c => c.Client)
                .Where(c => c.ClientId == clientId && c.IsDeleted == false)
                .OrderByDescending(c => c.Created)
                .ToListAsync();
        }

        // get contracts by clientId with pagination
        public async Task<(List<Contract> Items, int TotalCount)> GetByClientIdPaginationAsync(Guid clientId, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Contract>()
                .Include(c => c.Client)
                .Where(c => c.ClientId == clientId && c.IsDeleted == false)
                .OrderByDescending(c => c.Created)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
