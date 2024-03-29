﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TalkToApi.Helpers;
using TalkToApi.Helpers.Contants;
using TalkToApi.Repositories.Context;
using TalkToApi.V1.Helpers.Swagger;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            #region AutoMapper
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile(new DTOMapperProfile());
            });
            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            #endregion

            services.Configure<ApiBehaviorOptions>(op =>
            {
                op.SuppressModelStateInvalidFilter = true;
            });


            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();

            services.AddDbContext<TalkToApiContext>(op =>
            {
                op.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddCors(cfg =>
            {
                cfg.AddDefaultPolicy(policy =>
                {
                    policy
                        .WithOrigins("https://localhost:44352", "http://localhost:44352")
                        .WithMethods("GET", "POST")
                        .WithHeaders("Accept", "Authorization")
                        .SetIsOriginAllowedToAllowWildcardSubdomains(); // Aceita todos os subdominios ex.: empresa.com.br, app.empresa.com.br, web.empresa.com.br
                });

                // habilitar todos os sites com restrição
                cfg.AddPolicy("AnyOrigin", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .WithMethods("GET")
                        .AllowAnyHeader();
                });
            });

            services.AddMvc(cfg =>
            {
                cfg.ReturnHttpNotAcceptable = true;
                cfg.InputFormatters.Add(new XmlSerializerInputFormatter(cfg));
                cfg.OutputFormatters.Add(new XmlSerializerOutputFormatter());

                var jsonOutputFormat = cfg.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormat != null)
                {
                    jsonOutputFormat.SupportedMediaTypes.Add(CustomMediaType.Hateoas);

                }
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddJsonOptions(
                options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddApiVersioning(cfg =>
            {
                cfg.ReportApiVersions = true;
                //cfg.ApiVersionReader = new HeaderApiVersionReader("api-version"); // Para habilitar pasar a versão pelo header
                cfg.AssumeDefaultVersionWhenUnspecified = true;
                cfg.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddSwaggerGen(cfg =>
            {
                cfg.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    In = "header",
                    Type = "apiKey",
                    Description = "Informe o JSON Web Token para autenticar.",
                    Name = "Authorization"
                });

                var security = new Dictionary<string, IEnumerable<string>>()
                {
                    { "Bearer", new string[] { } }
                };
                cfg.AddSecurityRequirement(security);


                cfg.ResolveConflictingActions(apiDescription => apiDescription.First());
                cfg.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "Talk To API - V1.0",
                    Version = "v1.0",
                });

                var CaminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
                var NomeProjeto = $"{PlatformServices.Default.Application.ApplicationName}.xml";
                var CaminhoArquivoXMLComentario = Path.Combine(CaminhoProjeto, NomeProjeto);

                cfg.IncludeXmlComments(CaminhoArquivoXMLComentario);

                cfg.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });

                cfg.OperationFilter<ApiVersionOperationFilter>();
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
            {
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 5;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireNonAlphanumeric = false; // sem caracteres especiais
            })
                .AddEntityFrameworkStores<TalkToApiContext>()
                .AddDefaultTokenProviders();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("key-api-jwt-minhas-tarefas-lucas")); // Recomendad criar no appsettings.json

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false, // caso seja ncessario validar, é peciso adicionar na geração do token
                    ValidateAudience = false, // caso seja ncessario validar, é peciso adicionar na geração do token
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("key-api-jwt-minhas-tarefas-lucas")) // Recomendado criar no appsettings.json
                };
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build()
                );
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseStatusCodePages();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseCors(); // Desabilite quando usaratributos EnableCors/DisableCors
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(cfg =>
            {
                cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Tolk To Api - V1.0");
                cfg.RoutePrefix = string.Empty;
            });
        }
    }
}
