using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpHomeServer.Data;
using SharpHomeServer.EmailSender;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SharpHomeServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<RavenDbOptions>(Configuration.GetSection(
                                                RavenDbOptions.RavenDb));

            services.Configure<EmailSenderOptions>(Configuration.GetSection(
                                    EmailSenderOptions.EmailSender));


            services.Configure<ChartOptions>(Configuration.GetSection("Layout"));

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddScoped<TimeZoneService>();

            services.AddSingleton<IRavenDbDocumentStore, RavenDbDocumentStore>();
            services.AddSingleton<IReadingProvider, ReadingProvider>();
            services.AddSingleton<IChartDataProvider, ChartDataProvider>();
            services.AddHostedService<IndexDeployer>();
            services.AddHostedService<DailyStatsMailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
