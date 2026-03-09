using CleanOpsAi.Modules.ClientManagement.Application.Dtos;
using CleanOpsAi.Modules.ClientManagement.Application.Interfaces;


namespace CleanOpsAi.Modules.ClientManagement.Application.Services
{
    public class ClientService
    {
        private readonly IClientRepository _clientRepository;
        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<List<ClientResponse>> GetByIdAsync(Guid id)
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return null;
            }
            return new List<ClientResponse> { new ClientResponse { Id = client.Id, Name = client.Name, Email = client.Email } };
        }

        public async Task<List<ClientResponse>> GetAllAsync()
        {
            var clients = await _clientRepository.GetAllAsync();
            return clients.Select(c => new ClientResponse { Id = c.Id, Name = c.Name, Email = c.Email }).ToList();
        }

    }
}
