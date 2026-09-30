using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProektIntegrirani.Domain.Dto;
using ProektIntegrirani.Domain.Models;
using ProektIntegrirani.Service.Interface;
using ProektIntegrirani.Web.Extensions;
using ProektIntegrirani.Web.Middlewares;

namespace ProektIntegrirani.Web.Controllers;

// Inbound API: external systems push squads; they are stored as Pending and processed in the background.
[ApiController]
[Route("api/external/squads")]
[EnableRateLimiting(RateLimitingExtensions.ExternalApiPolicy)]
public class ExternalSquadsController : ControllerBase
{
    private readonly IInboundSquadEntryService _service;

    public ExternalSquadsController(IInboundSquadEntryService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveSquad([FromBody] InboundSquadRequest request)
    {
        if (HttpContext.Items[ApiKeyAuthMiddleware.ApiClientItemKey] is not ApiClient apiClient)
        {
            return Unauthorized();
        }

        var payload = JsonSerializer.Serialize(request);
        var entry = await _service.CreateAsync(payload, apiClient.Id);

        return AcceptedAtAction(nameof(GetStatus), new { id = entry.Id }, new
        {
            id = entry.Id,
            status = entry.Status.ToString().ToLower(),
            message = "Squad queued for processing"
        });
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var entry = await _service.GetByIdAsync(id);

        // A client only sees its own submissions.
        if (HttpContext.Items[ApiKeyAuthMiddleware.ApiClientItemKey] is not ApiClient apiClient
            || entry.ApiClientId != apiClient.Id)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = entry.Id,
            status = entry.Status.ToString().ToLower(),
            receivedAt = entry.ReceivedAt,
            processedAt = entry.ProcessedAt,
            managerId = entry.ManagerId,
            error = entry.ErrorMessage
        });
    }
}
