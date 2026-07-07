using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace Employee.API.Extensions
{
    public static class ApiVersioningExtensions
    {
        public static IServiceCollection AddApiVersioningWithSwagger(
            this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-API-Version"),
                    new MediaTypeApiVersionReader("X-API-Version"));
            })
            .AddMvc()
            .AddApiExplorer();

            return services;
        }

        public static IServiceCollection AddSwaggerWithVersioning(
            this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Employee Management API - V1",
                    Version = "v1",
                    Description = "Employee Management Microservice API V1",
                    Contact = new OpenApiContact
                    {
                        Name = "Support Team",
                        Email = "support@company.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT"
                    }
                });

                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "Employee Management API - V2",
                    Version = "v2",
                    Description = "Employee Management Microservice API V2",
                    Contact = new OpenApiContact
                    {
                        Name = "Support Team",
                        Email = "support@company.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT"
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] { }
                    }
                });

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }
    }
}
