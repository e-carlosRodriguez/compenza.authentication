using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using compenza.authentication.percistance;
using System.Reflection;
using compenza.authentication.application.Utilities;
using compenza.identity.domain.Configuration;

namespace compenza.authentication.application
{
    public static class ApplicationExtenssions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPercistance(configuration);

            services.AddMediatR(options => options.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.Configure<EncryptSecrets>(configuration.GetSection("EncryptSecrets"));
            services.AddTransient<IStringsEncrypter, Encrypt>();
            services.AddTransient<ITokenProvider, TokenProvider>();

            return services;
        }
    }
}
