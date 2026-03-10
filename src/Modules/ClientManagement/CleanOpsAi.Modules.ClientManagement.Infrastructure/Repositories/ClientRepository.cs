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
    public class ClientRepository : IClientRepository
    {
        private readonly ClientManagementDbContext _dbContext;
        public ClientRepository(ClientManagementDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // get Client by id
        public async Task<Client> GetByIdAsync(Guid id)
        {
            var client = _dbContext.Set<Client>().FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
            return client;

        }

        // get all Clients
        public async Task<List<Client>> GetAllAsync()
        {
            var clients = await _dbContext.Set<Client>()
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return clients;
        }

        // get all Clients with pagination
        public async Task<(List<Client> Items, int TotalCount)> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<Client>().AsQueryable().Where(c => c.IsDeleted == false).OrderByDescending(c => c.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // add Client
        public async Task<int> CreateAsync(Client client)
        {
            _dbContext.Set<Client>().Add(client);
            return await _dbContext.SaveChangesAsync();
        }

        // update Client
        public async Task<int> UpdateAsync(Client client)
        {
            _dbContext.Set<Client>().Update(client);
            return await _dbContext.SaveChangesAsync();
        }

        // delete Client
        public async Task<int> DeleteAsync(Guid id)
        {
            var client = _dbContext.Set<Client>().FirstOrDefault(c => c.Id == id);
            if (client != null)
            {
                var newClient = new Client
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email,
                    CreatedBy = client.CreatedBy,
                    Created = client.Created,
                    LastModifiedBy = client.LastModifiedBy,
                    LastModified = client.LastModified,
                    IsDeleted = true
                };
                _dbContext.Set<Client>().Update(client);
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }
    }
}
