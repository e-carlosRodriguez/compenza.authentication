using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using compenza.authentication.domain.Configure;
using compenza.authentication.percistance.Interfaces;
using compenza.authentication.percistance.Repository;

namespace compenza.authentication.percistance
{
    public static class PercistanceExtenssions
    {
        public static IServiceCollection AddPercistance(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Settings>(configuration.GetSection("Settings"));
            services.AddTransient<ICompenzaDbContext, CompenzaDbContext>();
            services.AddTransient<ILoginRepository, LoginRepository>();

            return services;
        }
    }
}
