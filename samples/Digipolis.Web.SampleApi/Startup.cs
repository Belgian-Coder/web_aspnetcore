﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Digipolis.Web;
using Digipolis.Web.SampleApi.Configuration;
using AutoMapper;
using Digipolis.Web.SampleApi.Data;
using Digipolis.Web.SampleApi.Logic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Digipolis.Web.Swagger;
using Digipolis.Web.Startup;
using Digipolis.Web.Api;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;

namespace Digipolis.Web.SampleApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc()
                .AddApiExtensions(Configuration.GetSection("ApiExtensions"), x =>
                {
                    //Override settings made by the appsettings.json
                    x.PageSize = 10;
                    x.DisableVersioning = false;
                });

            services.AddGlobalErrorHandling<ApiExceptionMapper>();

            services.AddAuthorization();

            // Add Swagger extensions
            services.AddSwaggerGen<ApiExtensionSwaggerSettings>(o =>
            {
                o.SwaggerDoc(Versions.V1, new Info
                {
                    //Add Inline version
                    Version = Versions.V1,
                    Title = "API V1",
                    Description = "Description for V1 of the API",
                    Contact = new Contact { Email = "info@digipolis.be", Name = "Digipolis", Url = "https://www.digipolis.be" },
                    TermsOfService = "https://www.digipolis.be/tos",
                    License = new License
                    {
                        Name = "My License",
                        Url = "https://www.digipolis.be/licensing"
                    },
                });

                o.SwaggerDoc("v2", new Version2());
            });

            //Register Dependencies for example project
            services.AddScoped<IValueRepository, ValueRepository>();
            services.AddScoped<IValueLogic, ValueLogic>();

            //Add AutoMapper
            services.AddAutoMapper();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("StatusMonitoringPolicy", policy =>
                {
                    policy.AuthenticationSchemes = new string[] { "JwtHeaderAuth" };
                    policy.AddRequirements(new StatusRequirementHandler());
                });
            });
        }

        public class StatusRequirementHandler : AuthorizationHandler<StatusRequirementHandler>, IAuthorizationRequirement
        {
            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StatusRequirementHandler requirement)
            {
                //var roles = new[] { "Admin", "Admin2", "Admin3" };  //Get From DB.
                //var userIsInRole = roles.Any(role => context.User.IsInRole(role));
                //if (!userIsInRole)
                //{
                //    context.Fail();
                //}

                context.Succeed(requirement);

                return Task.CompletedTask;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Enable Api Extensions
            app.UseApiExtensions();


            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "docs/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
            });

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/docs/v1/swagger.json", "V1 Documentation");
                options.SwaggerEndpoint("/docs/v2/swagger.json", "V2 Documentation");
            });

            app.UseSwaggerUiRedirect();

        }
    }
}
