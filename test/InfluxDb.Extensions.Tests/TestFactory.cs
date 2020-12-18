using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InfluxDb.Extensions.Tests {
    public class TestFactory {

        public const string Influx_Url = "http://192.168.1.22:8086";
        public const string Influx_Database = "RMDP";
        protected readonly TestServer TestServer;

        public IServiceScope Scope { get; private set; }

        public TestFactory () : this (null) {

        }
        public TestFactory (Action<IServiceCollection> configServices) {
            var builder = WebHost.CreateDefaultBuilder ()
                .ConfigureServices (s => {
                    configServices?.Invoke (s);
                    ConfigureServices(s);
                }).Configure(a=>{
                    
                });

            TestServer = new TestServer (builder);
            this.Scope = TestServer.Host.Services.CreateScope ();
        }

        public void ConfigureServices (IServiceCollection services) {
            services.AddMemoryCache ();
            services.AddInfluxContext (o => {
                o.Url = Influx_Url;
                o.Database = Influx_Database;
            });
            services.AddLogging (b => b.AddConsole ().AddDebug ()
                .SetMinimumLevel (LogLevel.Debug));
        }

        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            
        }

        public TController GetController<TController> () where TController : ControllerBase {
            return Services.GetService<TController> ();
        }

        public TService GetService<TService> () {
            return Services.GetService<TService> ();
        }

        public IServiceScope NewScope () {
            this.Scope = TestServer.Host.Services.CreateScope ();
            return Scope;
        }

        public IServiceProvider Services => Scope.ServiceProvider;
    }

    public class StartUp{

    }
}