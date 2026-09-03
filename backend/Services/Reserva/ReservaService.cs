using AutoMapper;
using DTO;
using Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using Repository;

namespace Services;

public class ReservaService : IReservaService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReservaService> _logger;
    private readonly IAuditoriaService? _auditoriaService;
    private readonly ICacheService? _cacheService;

    public ReservaService(
        IReservaRepository reservaRepository,
        IAlunoRepository alunoRepository,
        ILivroRepository livroRepository,
        IMapper mapper,
        ILogger<ReservaService>? logger = null,
        IAuditoriaService? auditoriaService = null,
        ICacheService? cacheService = null)
    {
        _reservaRepository = reservaRepository;
        _alunoRepository = alunoRepository;
        _livroRepository = livroRepository;
        _mapper = mapper;
        _logger = logger ?? NullLogger<ReservaService>.Instance;
        _auditoriaService = auditoriaService;
        _cacheService = cacheService;
    }

    public async Task<ReservaResponseDTO> AddReservaAsync(CriarReservaDTO dto)
    {
        _logger.LogInformation("Solicitando reserva para AlunoId {AlunoId} e LivroId {LivroId}", dto.AlunoId, dto.LivroId);

        var aluno = await _alunoRepository.GetAlunoByIdAsync(dto.AlunoId);
        if (aluno == null)
        {
            _logger.LogWarning("Falha na reserva: Aluno com ID {AlunoId} não encontrado", dto.AlunoId);
            throw new NotFoundException($"Aluno com id {dto.AlunoId} não encontrado.");
        }

        var livro = await _livroRepository.GetLivroByIdAsync(dto.LivroId);
        if (livro == null)
        {
            _logger.LogWarning("Falha na reserva: Livro com ID {LivroId} não encontrado", dto.LivroId);
            throw new NotFoundException($"Livro com id {dto.LivroId} não encontrado.");
        }

        if (livro.Quantidade > 0)
        {
            _logger.LogWarning("Tentativa de reserva para livro com estoque disponível ({Quantidade})", livro.Quantidade);
            throw new ConflictException("O livro está disponível em estoque para empréstimo imediato. Não é necessário reservar.");
        }

        bool hasActiveReservation = await _reservaRepository.ExistsReservaAtivaAsync(dto.AlunoId, dto.LivroId);
        if (hasActiveReservation)
        {
            _logger.LogWarning("Aluno {AlunoId} já possui uma reserva ativa para o livro {LivroId}", dto.AlunoId, dto.LivroId);
            throw new ConflictException("O aluno já possui uma reserva ativa para este livro.");
        }

        var reserva = new Reserva
        {
            Id = Guid.NewGuid(),
            AlunoId = dto.AlunoId,
            Aluno = aluno,
            LivroId = dto.LivroId,
            Livro = livro,
            DataReserva = DateTime.UtcNow,
            Status = StatusReserva.Ativa
        };

        var reservaCriada = await _reservaRepository.AddReservaAsync(reserva);

        var fila = await _reservaRepository.GetFilaEsperaByLivroIdAsync(dto.LivroId);
        int posicao = fila.FindIndex(r => r.Id == reservaCriada.Id) + 1;

        _logger.LogInformation("Reserva criada com sucesso (ID: {Id}). Posição na fila: {Posicao}", reservaCriada.Id, posicao);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                "CRIACAO_RESERVA",
                $"Reserva ID {reservaCriada.Id} criada para aluno '{aluno.Nome}' do livro '{livro.Titulo}'. Posição: {posicao}");
        }

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("dashboard:stats");
        }

        var response = _mapper.Map<ReservaResponseDTO>(reservaCriada);
        response.PosicaoFila = posicao;
        return response;
    }

    public async Task<List<ReservaResponseDTO>> GetFilaEsperaAsync(Guid livroId)
    {
        _logger.LogInformation("Buscando fila de espera para o livro ID {LivroId}", livroId);

        var livro = await _livroRepository.GetLivroByIdAsync(livroId);
        if (livro == null)
        {
            _logger.LogWarning("Livro com ID {LivroId} não encontrado", livroId);
            throw new NotFoundException($"Livro com id {livroId} não encontrado.");
        }

        var fila = await _reservaRepository.GetFilaEsperaByLivroIdAsync(livroId);
        var result = new List<ReservaResponseDTO>();

        for (int i = 0; i < fila.Count; i++)
        {
            var dto = _mapper.Map<ReservaResponseDTO>(fila[i]);
            dto.PosicaoFila = i + 1;
            result.Add(dto);
        }

        return result;
    }

    public async Task<List<ReservaResponseDTO>> GetByAlunoIdAsync(Guid alunoId)
    {
        _logger.LogInformation("Buscando reservas do aluno com ID {AlunoId}", alunoId);
        var reservas = await _reservaRepository.GetByAlunoIdAsync(alunoId);
        return _mapper.Map<List<ReservaResponseDTO>>(reservas);
    }

    public async Task<List<ReservaResponseDTO>> GetAllReservasAsync()
    {
        _logger.LogInformation("Buscando todas as reservas cadastradas para gestão");
        var reservas = await _reservaRepository.GetAllReservasAsync();
        var result = new List<ReservaResponseDTO>();

        foreach (var r in reservas)
        {
            var dto = _mapper.Map<ReservaResponseDTO>(r);
            if (r.Status == StatusReserva.Ativa)
            {
                var fila = await _reservaRepository.GetFilaEsperaByLivroIdAsync(r.LivroId);
                dto.PosicaoFila = fila.FindIndex(f => f.Id == r.Id) + 1;
            }
            result.Add(dto);
        }

        return result;
    }

    public async Task CancelarReservaAsync(Guid reservaId)
    {
        _logger.LogInformation("Tentando cancelar reserva ID {ReservaId}", reservaId);
        var reserva = await _reservaRepository.GetReservaByIdAsync(reservaId);
        if (reserva == null)
        {
            _logger.LogWarning("Reserva {ReservaId} não encontrada para cancelamento", reservaId);
            throw new NotFoundException($"Reserva com ID '{reservaId}' não encontrada.");
        }

        reserva.Status = StatusReserva.Cancelada;
        await _reservaRepository.UpdateReservaAsync(reserva);

        if (_auditoriaService != null)
        {
            await _auditoriaService.RegistrarAcaoAsync(
                "CANCELAMENTO_RESERVA",
                $"Reserva ID {reserva.Id} cancelada para o livro '{reserva.Livro?.Titulo}' e aluno '{reserva.Aluno?.Nome}'");
        }

        if (_cacheService != null)
        {
            await _cacheService.RemoveAsync("dashboard:stats");
        }
    }
}
