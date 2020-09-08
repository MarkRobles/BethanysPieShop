using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BethanysPieShop.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BethanysPieShop
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPieRepository, MockPieRepository>();
            services.AddScoped<ICategoryRepository, MockCategoryRepository>();
            //Add suport  for MVC
            services.AddControllersWithViews();
            //NET CORE 2.0
          //  services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseHttpsRedirection();
            //By default will search in directoy wwwrot
            app.UseStaticFiles();
            //UseRouting and UseEndpoints enables to MVC handle incoming request
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                //We don't want to respond to wvery request with hello world so we comment out this.
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});


                endpoints.MapControllerRoute(
                  name:"default",
                  pattern:"{controller=Home}/{action=Index}/{id?}"
                    );
            });
        }
    }
}
