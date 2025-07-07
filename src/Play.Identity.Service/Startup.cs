using System;
using System.IO;
using System.Reflection;
using GreenPipes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.HealthChecks;
using Play.Common.MassTransit;
using Play.Common.Settings;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Exception;
using Play.Identity.Service.HealthChecks;
using Play.Identity.Service.HostedServices;
using Play.Identity.Service.Settings;

namespace Play.Identity.Service
{
    public class Startup
    {
        public const string AllowedOriginSettings = "AllowedOrigin";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // mongodb as integration point with identity
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            var serviceSettings = Configuration.GetSection("ServiceSettings").Get<ServiceSettings>();
            var mongoDbSettings = Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
            var identityServerSettings = Configuration.GetSection("IdentityServerSettings").Get<IdentityServerSettings>(); 
            
            
            services.Configure<IdentitySettings>(Configuration.GetSection(nameof(IdentitySettings)))
                .AddDefaultIdentity<ApplicationUser>()
                .AddRoles<ApplicationRole>()
                .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
                    mongoDbSettings.ConnectionString,
                    serviceSettings.ServiceName
                    );

            services.AddMassTransitWithMessageBroker(Configuration, retryConfigurator =>
            {
                retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                retryConfigurator.Ignore(typeof(UnknownUserException));
                retryConfigurator.Ignore(typeof(InsufficientFundException));
            });
            
            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                    options.KeyManagement.KeyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddInMemoryApiScopes(identityServerSettings.ApiScopes)
                .AddInMemoryApiResources(identityServerSettings.ApiResources)
                .AddInMemoryClients(identityServerSettings.Clients)
                .AddInMemoryIdentityResources(identityServerSettings.IdentityResources);

            services.AddLocalApiAuthentication();
            services.AddControllers();
            services.AddHostedService<IdentitySeedHostedService>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Identity.Service", Version = "v1" });
            });
            services.AddHealthChecks()
                .AddMongoDb();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Identity.Service v1"));
                app.UseCors(builder =>
                {
                    builder.WithOrigins(Configuration[AllowedOriginSettings])
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }); 
            }

            app.UseHttpsRedirection();
            // Dynamically sets the app's request base path using a value from config
            app.Use((context, next) =>
            {
               var identitySettings = Configuration.GetSection(nameof(IdentitySettings))
                                        .Get<IdentitySettings>();
               context.Request.PathBase = new PathString(identitySettings.PathBase);
               return next();
            });
            app.UseStaticFiles();
            
            app.UseRouting();
            
            // after routing and before authorizing, we insert identity server middleware
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapPlayEconomyHealthChecks();
            });
        }
    }
}
