using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDTO<AuditoriaResponseDTO>>> GetAuditoria([FromQuery] PaginationParamsDTO pagination)
    {
        var logs = await _auditoriaService.GetPagedAsync(pagination);
        return Ok(logs);
    }
}
