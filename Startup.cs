/*
 * Copyright (c) 2019-2025, Incendi (info@incendi.no)
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

using System;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Spark.Engine;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Spark.Engine.Service.FhirServiceExtensions;
using Spark.Store.MongoDB.Extensions;

namespace spark_example;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        // Set up a default policy for CORS that accepts any origin, method and header.
        // only for test purposes.
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyMethod();
                    policy.AllowAnyHeader();
                }
            )
        );

        services.Replace(
            ServiceDescriptor.Transient<ICapabilityStatementService>(serviceProvider => new CapabilityStatementService(
                    serviceProvider.GetRequiredService<IFhirModel>(),
                    new ServerVersion(1, 0, 0),
                    FHIRVersion.N0_4_0
                )
            )
        );

        services.AddFhirWithMvc(
            new SparkSettings
            {
                Endpoint = new Uri("https://localhost:5001")
            },
            options =>
            {
                options.EnableEndpointRouting = false;
                options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
                options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();
            }
        );

        services.AddMongoFhirStore(
            new StoreSettings
            {
                ConnectionString = "mongodb://localhost/spark"
            }
        );
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<IgnoreRouteMiddleware>();
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseFhirWithMvc(r => r.MapRoute(name: "default", template: "{controller}/{id?}"));
    }
}
