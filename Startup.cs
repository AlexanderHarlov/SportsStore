using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SportsStore.Models;

namespace SportsStore
{
    public class Startup
    {
        //Configuration - includes appsettings.json contents
        private IConfiguration Configuration { get; set; }
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // Dependancy injection
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<StoreDbContext>(opts =>
            {
                opts.UseSqlServer(Configuration["ConnectionStrings:SportsStoreConnection"]);
            });
            //Each HTTP req gets its own repository object, dependency injection
            services.AddScoped<IStoreRepository, EFStoreRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //Display exception details. Not to be used in prod
            app.UseDeveloperExceptionPage(); 
            // Enable serving static content from wwwroot
            app.UseStaticFiles();
            // Endpoint routing
            app.UseRouting();
            // Add message for HTTP res that dont have body (i.e. 404)
            app.UseStatusCodePages();
            // Endpoint routing
            app.UseEndpoints(endpoints =>
            {
                //MVC is source of endpoints
                endpoints.MapDefaultControllerRoute();
            });
            //Ensure database is populated
            SeedData.EnsurePopulated(app);
        }
    }
}
