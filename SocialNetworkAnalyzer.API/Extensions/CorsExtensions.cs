using Microsoft.Extensions.DependencyInjection;

namespace SocialNetworkAnalyzer.API.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            // Add ReactAppPolicy for production
            services.AddCors(options =>
            {
                options.AddPolicy("ReactAppPolicy",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            // Add AllowAll policy for development
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            return services;
        }
    }
}
