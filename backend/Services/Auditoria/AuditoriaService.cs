using AutoMapper;
using DTO;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using Repository;

namespace Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditoriaService> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AuditoriaService(
        IAuditoriaRepository auditoriaRepository,
        IMapper mapper,
        ILogger<AuditoriaService>? logger = null,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _auditoriaRepository = auditoriaRepository;
        _mapper = mapper;
        _logger = logger ?? NullLogger<AuditoriaService>.Instance;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RegistrarAcaoAsync(string acao, string detalhes, string usuario = "Sistema")
    {
        if (usuario == "Sistema" && _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userEmail = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value
                            ?? _httpContextAccessor.HttpContext.User.Identity?.Name;
            if (!string.IsNullOrEmpty(userEmail))
            {
                usuario = userEmail;
            }
        }

        _logger.LogInformation("Registrando auditoria: Usuário='{Usuario}', Ação='{Acao}', Detalhes='{Detalhes}'",
            usuario, acao, detalhes);

        var auditoria = new Auditoria
        {
            Id = Guid.NewGuid(),
            Usuario = usuario,
            Acao = acao,
            Detalhes = detalhes,
            DataHora = DateTime.UtcNow
        };

        await _auditoriaRepository.RegistrarAsync(auditoria);
    }

    public async Task<PagedResultDTO<AuditoriaResponseDTO>> GetPagedAsync(PaginationParamsDTO paginationParams)
    {
        _logger.LogInformation("Buscando registros de auditoria paginados - Página {PageNumber}, Tamanho {PageSize}",
            paginationParams.PageNumber, paginationParams.PageSize);

        var (items, totalCount) = await _auditoriaRepository.GetPagedAsync(paginationParams.PageNumber, paginationParams.PageSize);
        var mappedItems = _mapper.Map<List<AuditoriaResponseDTO>>(items);

        return new PagedResultDTO<AuditoriaResponseDTO>(mappedItems, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }
}
