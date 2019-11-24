using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Spark.Engine;
using Spark.Engine.Extensions;
using Spark.Mongo.Extensions;

namespace spark_example
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Set up a default policy for CORS that accepts any origin, method and header.
            // only for test purposes.
            // services.AddCors(options =>
            //     options.AddDefaultPolicy(policy =>
            //     {
            //         policy.AllowAnyOrigin();
            //         policy.AllowAnyMethod();
            //         policy.AllowAnyHeader();
            //     }));
            
            services.AddFhir(new SparkSettings
            {
                Endpoint = new System.Uri("https://localhost:5001/fhir")
            });
            services.AddMongoFhirStore(new StoreSettings
            {
                ConnectionString = "mongodb://localhost:27017/spark-r4"
            });

            services.AddMvc(options =>
            {
                options.InputFormatters.RemoveType<JsonPatchInputFormatter>();
                options.InputFormatters.RemoveType<JsonInputFormatter>();
                options.OutputFormatters.RemoveType<JsonOutputFormatter>();

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseFhir(r => r.MapRoute(name: "default", template: "{controller}/{id?}"));
        }
    }
}
