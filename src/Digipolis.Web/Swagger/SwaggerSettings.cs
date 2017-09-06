﻿using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Digipolis.Web.Swagger
{
    public abstract class SwaggerSettings<TSwaggerResponseDefinitions> where TSwaggerResponseDefinitions : SwaggerResponseDefinitions
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.OperationFilter<TSwaggerResponseDefinitions>();
            var xmlPath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, PlatformServices.Default.Application.ApplicationName + ".xml");
            if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
            options.OperationFilter<AddFileUploadParams>();
            options.OperationFilter<AddConsumeProducesValues>();
            options.DocumentFilter<SetVersionInPaths>();
            options.SchemaFilter<PagedResultSchemaFilter>();
            options.DocumentFilter<EndPointPathsAndParamsToLower>();
            Configuration(options);
        }

        protected abstract void Configuration(SwaggerGenOptions options);
    }
}