using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
using System.Text;
using IPRESS.Application.Interfaces;
using IPRESS.Application.Services;
using IPRESS.Domain.Interfaces;
using IPRESS.Infrastructure.Persistence;
using IPRESS.API.Services;

namespace IPRESS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 50 * 1024 * 1024);

            builder.Services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 50 * 1024 * 1024;
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartHeadersLengthLimit = int.MaxValue;
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAuditUserIdProvider, HttpContextAuditUserIdProvider>();

            builder.Services.AddDbContext<IpressDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.UseCompatibilityLevel(120);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(30);
                    }));
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
            builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IPRESS API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Pegue aquí solo el token JWT (sin escribir 'Bearer'). Lo obtiene al hacer login en /api/auth/login.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                        )
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient",
                    policy => policy.SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            var app = builder.Build();

            app.UseResponseCompression();

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                app.Logger.LogCritical(ex, "Excepción no controlada del proceso. El proceso puede terminar.");
            };
            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                app.Logger.LogError(e.Exception, "Excepción no observada en Task.");
                e.SetObserved();
            };

            app.Use(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Error no controlado en la solicitud {Path}", context.Request.Path);
                    if (!context.Response.HasStarted)
                    {
                        try
                        {
                            context.Response.StatusCode = 500;
                            context.Response.ContentType = "application/json";
                            var msg = app.Environment.IsDevelopment() ? ex.Message : "Error interno del servidor.";
                            await context.Response.WriteAsJsonAsync(new { message = msg });
                        }
                        catch (Exception writeEx)
                        {
                            app.Logger.LogError(writeEx, "No se pudo escribir la respuesta de error.");
                        }
                    }
                }
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowClient");
            // No forzar HTTPS en desarrollo (evita problemas con certificados)
            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
