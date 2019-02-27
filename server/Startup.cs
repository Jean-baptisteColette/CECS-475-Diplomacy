using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diplomacy.Data.Entities;
using Diplomacy.Helpers;
using Diplomacy.Models;
using Diplomacy.Services;
using Dipomacy.Data;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

namespace Diplomacy
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            // Setup configuration file
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true).AddEnvironmentVariables();

            this.Configuration = builder.Build();

            Configuration = configuration;
        }
        public IHostingEnvironment Env { get; }
        public IConfiguration Configuration { get; }
        private const string SecretKey = "YZWNfqaakYN2E8sgBkBItHlOHEj7EMat";
        private readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddDbContext<DiplomacyContext>(options =>
                                                   options.UseSqlServer(Configuration.GetConnectionString("DiplomacyConnectionString")));

            // HangFire is used to perform background processing.                                        
            services.AddHangfire(configuration => configuration.UseMemoryStorage());

            // Swagger is a GUI for API Documentations that display all available requests in the server.
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "Diplomacy - HTTP API",
                    Version = "v1",
                    Description = "A Strategy Board Game."
                });
            });

            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));
            var tokenValidationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                ClockSkew = TimeSpan.Zero,

                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                // Specify the key used to sign the token:
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                // Ensure the token hasn't expired:
                RequireExpirationTime = false,
                ValidateLifetime = true
            };

            // Add Identity
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<DiplomacyContext>()
                .AddDefaultTokenProviders();

            //services.AddAuthentication().AddCookie().AddJwtBearer();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
            });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                options.SignIn.RequireConfirmedEmail = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Jti;

            });

            // Adding IdentityServer4
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddConfigurationStore(options =>
                    {
                        options.ConfigureDbContext = builder => builder.UseSqlServer(Configuration.GetConnectionString("DiplomacyConnectionString"));
                    })
                .AddOperationalStore(options =>
                    {
                        options.ConfigureDbContext = builder => builder.UseSqlServer(Configuration.GetConnectionString("DiplomacyConnectionString"));
                        options.EnableTokenCleanup = true;
                        options.TokenCleanupInterval = 3000;
                    })
                .AddAspNetIdentity<User>();

            services.AddScoped<IJwtFactory, JwtFactory>();

            services.AddAuthorization(options =>
                        {
                            options.AddPolicy("ApiUser", policy => policy.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
                        });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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

            app.UseHangfireServer();
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseSession();

            app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Diplomacy API V1");
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
