﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MinhasTarefasAPI.Database;
using MinhasTarefasAPI.Models;
using MinhasTarefasAPI.Repositories;
using MinhasTarefasAPI.Repositories.Contracts;

namespace MinhasTarefasAPI
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
            services.Configure<ApiBehaviorOptions>(op =>
            {
                op.SuppressModelStateInvalidFilter = true;
            });
            services.AddDbContext<MinhasTarefasContext>(op =>
            {
                op.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            /* Repositories */
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<ITarefaRepository, TarefaRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .AddJsonOptions(
                        options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    );

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MinhasTarefasContext>()
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("key-api-jwt-minhas-tarefas-lucas")) // Recomendad criar no appsettings.json
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePages();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
