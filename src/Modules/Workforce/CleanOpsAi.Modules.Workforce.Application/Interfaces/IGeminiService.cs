using CleanOpsAi.Modules.Workforce.Application.Dtos.Nlps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanOpsAi.Modules.Workforce.Application.Interfaces
{
    public interface IGeminiService
    {
        Task<string> ChatAsync(string message);
        Task<WorkerFilterNlpResult> ParseWorkerFilterAsync(string query, CancellationToken ct = default);
    }
}
