namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Services
{
    internal static class ContractScanPromptBuilder
    {
        internal static string BuildUserPrompt(string contractText) =>
            $$"""
            You are an expert system for extracting structured data from cleaning-service contracts.

            Read the contract text below and extract ALL Service Level Agreement (SLA) information.
            Group the results by location / area (e.g. "Lobby", "Restrooms", "Parking Lot").

            For each area produce:
            1. **SLA** — name and optional description.
            2. **Shifts** — each shift must have: name, startTime (HH:mm), endTime (HH:mm),
               requiredWorker (integer), breakTime (integer, minutes).
            3. **Tasks** — recurring tasks only (skip one-off or administrative items).
               Each task must have:
               - name: short English description
               - recurrenceType: one of Daily | Weekly | Monthly | Yearly
               - recurrenceConfig (JSON object with these fields):
                   * interval  (int, ≥1)
                   * daysOfWeek (array of 0-6, Sunday=0) — required for Weekly
                   * daysOfMonth (array of 1-31) — required for Monthly
                   * monthDays (array of {month,day} objects) — required for Yearly
               - sourceText: the exact sentence(s) from the contract that describe this task

            RULES:
            - The contract text may be in Vietnamese or English. Extract the names in their original language.
            - Only include tasks with a clear frequency (daily, weekly, monthly, quarterly…).
            - Ignore payment terms, penalties, legal clauses, signatures.
            - If you are uncertain about a field, omit it rather than guess.
            - Output ONLY valid JSON — no markdown fences, no extra prose.

            JSON SCHEMA:
            { 
              "slas": [
                {
                  "name": "string",
                  "description": "string | null",
                  "workAreaName": "string | null",
                  "shifts": [
                    {
                      "name": "string",
                      "startTime": "HH:mm",
                      "endTime": "HH:mm",
                      "requiredWorker": 0,
                      "breakTime": 0
                    }
                  ],
                  "tasks": [
                    {
                      "name": "string",
                      "recurrenceType": "Daily|Weekly|Monthly|Yearly",
                      "recurrenceConfig": {
                        "interval": 1,
                        "daysOfWeek": null,
                        "daysOfMonth": null,
                        "monthDays": [{"month": 1, "day": 1}]
                      },
                      "sourceText": "string | null"
                    }
                  ]
                }
              ],
              "warnings": ["string"]
            }

            CONTRACT TEXT:
            {{contractText}}
            """;
    }
}
