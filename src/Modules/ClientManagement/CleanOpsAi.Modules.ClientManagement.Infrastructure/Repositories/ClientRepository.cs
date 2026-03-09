using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;
using CleanOpsAi.Modules.ClientManagement.Domain.Entities;
using CleanOpsAi.Modules.ClientManagement.Infrastructure.Data;
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
            var client = _dbContext.Set<Client>().FirstOrDefault(c => c.Id == id);
            return client;

        }

        // get all Clients
        public async Task<List<Client>> GetAllAsync()
        {
            var clients = _dbContext.Set<Client>().ToList();
            return clients;
        }

        // get all Clients with pagination
        public async Task<List<Client>> GetAllPaginationAsync(int pageNumber, int pageSize)
        {
            var clients = _dbContext.Set<Client>()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return clients;
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
                _dbContext.Set<Client>().Remove(client);
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }
    }
}
