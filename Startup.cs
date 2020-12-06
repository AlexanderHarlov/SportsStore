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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

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
            services.AddScoped<IOrderRepository, EFOrderRepository>();
            //Use Razor Pages
            services.AddRazorPages();
            //Enable in-memory data store for the sessions
            services.AddDistributedMemoryCache();
            //Enable sessions - saving state for several requests
            services.AddSession();
            //All Cart related requests should be handled by SessionCart -> requests for Cart service
            //will be handled by creating SessionCart objects
            services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));
            //The same object will be used when IHttpContextAccessor is needed -> used to obtain the session
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //Add blazor server service
            services.AddServerSideBlazor();
            //Add identity db context
            services.AddDbContext<AppIdentityDbContext>(options =>
               options.UseSqlServer(
                   Configuration["ConnectionStrings:IdentityConnection"]));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsProduction())
            {
                //Use simple HTML to notify the user an error has ocurred
                app.UseExceptionHandler("/error");
            }
            else
            {
                //Display exception details. Not to be used in prod
                app.UseDeveloperExceptionPage();
                // Add message for HTTP res that dont have body (i.e. 404)
                app.UseStatusCodePages();
            }          
            // Enable serving static content from wwwroot
            app.UseStaticFiles();
            // Endpoint routing
            app.UseRouting();           
            //Assosiate requests with sessions
            app.UseSession();
            //Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();
            // Endpoint routing
            app.UseEndpoints(endpoints =>
            {
                //Add support for more apealing page paths
                // / -> First page of products for all categories
                // /Page2 -> Specified page of products for all categories
                // /Soccer -> First page of products for sprecified category
                // /Soccer/Page2 -> Specified page of products for sprecified category
                endpoints.MapControllerRoute("catpage",
                    "{category}/Page{productPage:int}",
                    new { Controller = "Home", action = "Index" });
                endpoints.MapControllerRoute("page",
                    "Page{productPage:int}",
                    new { Controller = "Home", action = "Index", productPage = 1 });
                endpoints.MapControllerRoute("category",
                    "{category}",
                    new { Controller = "Home", action = "Index", productPage = 1 });

                endpoints.MapControllerRoute("pagination",
                    "Products/Page{productPage}",
                    new { Controller = "Home", action = "Index", productPage = 1 });
                //MVC is source of endpoints
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/admin/{*catchall}", "/Admin/Index");
            });
            //Ensure database is populated
            SeedData.EnsurePopulated(app);
            IdentitySeedData.EnsurePopulated(app);
        }
    }
}
