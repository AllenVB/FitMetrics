using FitMetrics.Application.DTOs.Knowledge;
using FitMetrics.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitMetrics.API.Controllers;

public class KnowledgeController : ApiControllerBase
{
    private readonly IKnowledgeService _knowledge;

    public KnowledgeController(IKnowledgeService knowledge) => _knowledge = knowledge;

    [HttpGet]
    public async Task<ActionResult<List<KnowledgeEntryDto>>> GetAll(CancellationToken ct)
        => Ok(await _knowledge.GetAllAsync(ct));

    [HttpPost]
    public async Task<ActionResult<KnowledgeEntryDto>> Create(CreateKnowledgeEntryRequest request, CancellationToken ct)
        => Ok(await _knowledge.CreateAsync(request, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _knowledge.DeleteAsync(id, ct);
        return NoContent();
    }
}
