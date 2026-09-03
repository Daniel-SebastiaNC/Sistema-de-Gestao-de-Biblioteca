using Common;
using DataContext;
using Handlers;
using Mapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Middlewares;
using Repository;
using Services;
using System.Text;

DotEnv.Load();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT: Bearer {seu token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    var host = builder.Configuration["POSTGRES_HOST"] ?? "localhost";
    var port = builder.Configuration["POSTGRES_PORT"] ?? "5432";
    var db = builder.Configuration["POSTGRES_DB"] ?? "biblioteca";
    var user = builder.Configuration["POSTGRES_USER"] ?? "postgres";
    var pass = builder.Configuration["POSTGRES_PASSWORD"] ?? "postgres";
    connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
}

builder.Services.AddDbContext<BibliotecaContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Repositórios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IAutorRepository, AutorRepository>();
builder.Services.AddScoped<ILivroRepository, LivroRepository>();
builder.Services.AddScoped<IEmprestimoRepository, EmprestimoRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AlunoProfile>();
    cfg.AddProfile<EmprestimoProfile>();
    cfg.AddProfile<AutorProfile>();
    cfg.AddProfile<LivroProfile>();
    cfg.AddProfile<ReservaProfile>();
    cfg.AddProfile<AuditoriaProfile>();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Serviços
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAlunoService, AlunoService>();
builder.Services.AddScoped<IAutorService, AutorService>();
builder.Services.AddScoped<ILivroService, LivroService>();
builder.Services.AddScoped<IEmprestimoService, EmprestimoService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

// Configuração JWT
var jwtSecret = builder.Configuration["JWT_SECRET"]
                ?? builder.Configuration["Jwt:Secret"]
                ?? "BibliotecaChaveSuperSecretaParaAssinaturaJwt123456789";
var jwtIssuer = builder.Configuration["JWT_ISSUER"]
                ?? builder.Configuration["Jwt:Issuer"]
                ?? "BibliotecaApi";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"]
                  ?? builder.Configuration["Jwt:Audience"]
                  ?? "BibliotecaClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

var redisConnectionString = builder.Configuration["REDIS_CONNECTION_STRING"];
if (string.IsNullOrEmpty(redisConnectionString))
{
    var redisHost = builder.Configuration["REDIS_HOST"] ?? "localhost";
    var redisPort = builder.Configuration["REDIS_PORT"] ?? "6379";
    redisConnectionString = $"{redisHost}:{redisPort}";
}

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Biblioteca_";
});
builder.Services.AddScoped<ICacheService, RedisCacheService>();

var corsOrigin = builder.Configuration["CORS_ORIGIN"] ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsConfiguration", policy =>
    {
        policy.WithOrigins(corsOrigin)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("CorsConfiguration");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();
    context.Database.Migrate();

    if (app.Environment.IsDevelopment())
    {
        DbInitializer.Initialize(context);
    }
}

app.Run();
