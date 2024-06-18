using System;
using InfluxData.Net.InfluxDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InfluxDb.Extensions
{
    public static class InfluxContextExtensions {

        /// <summary>
        /// 注册 <see cref="IOptions{InfluxDbOptions}"/>,<see cref="ISerieContextFactory"/> 和 <see cref="IInfluxDbClient"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsBuilder"></param>
        public static void AddInfluxContext (this IServiceCollection services, Action<InfluxDbOptions> optionsBuilder) {
            services.Configure (optionsBuilder).AddOptions ();
            services.AddScoped<IInfluxDbClient> (s => s.GetService<IOptions<InfluxDbOptions>> ().Value.CreateNewClient ());
            services.AddScoped<ISerieContextFactory, SerieContextFactory> ();
        }
    }
}