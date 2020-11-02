using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BethanysPieShop.Auth;
using BethanysPieShop.Filters;
using BethanysPieShop.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Serilog;

namespace BethanysPieShop
{
    public class Startup
    {

        public IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //Let our app be aware of EF Core
            services.AddDbContext<AppDbContext>(options =>
                  options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<AppDbContext>();

            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

            //  services.AddDefaultIdentity<ApplicationUser>().AddEntityFrameworkStores<AppDbContext>();
            services.AddScoped<IPieRepository, PieRepository>();
            services.AddScoped<ICategoryRepository,CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPieReviewRepository, PieReviewRepository>();
            services.AddScoped<ShoppingCart>(sp => ShoppingCart.GetCart(sp));

            //specify options for the anti forgery here
            //services.AddAntiforgery(opts => { opts.RequireSsl = true; });
            services.AddAntiforgery();

            ////anti forgery as global filter
            services.AddMvc(options =>
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

            //LOCALIZATION!
            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString =
                    Configuration.GetConnectionString("DefaultConnection");
                options.SchemaName = "dbo";
                options.TableName = "TestCache";
            });

            services.AddMvc(
             config => { config.Filters.AddService(typeof(TimerAction)); }
             )
            .AddViewLocalization(
                   LanguageViewLocationExpanderFormat.Suffix,
                   opts => { opts.ResourcesPath = "Resources"; })
               .AddDataAnnotationsLocalization();

            //response compression with gzip 
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes =
                    ResponseCompressionDefaults.MimeTypes.Concat(new[] { "imagejpeg" });
            });

            services.Configure<GzipCompressionProviderOptions>
                (options => options.Level =
                    System.IO.Compression.CompressionLevel.Optimal);

            services.Configure<RequestLocalizationOptions>(
           options =>
           {
               var supportedCultures = new List<CultureInfo>
               {
                        new CultureInfo("fr"),
                        new CultureInfo("fr-FR"),
                        new CultureInfo("nl"),
                        new CultureInfo("nl-BE"),
                        new CultureInfo("en-US")
               };

               options.DefaultRequestCulture = new RequestCulture("en-US");
               options.SupportedCultures = supportedCultures;
               options.SupportedUICultures = supportedCultures;
           });

            //Get access to session objects in classes (by default only has access in controllers without AddHttpContextAccessor)
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddSession();


            //Filters
            services.AddScoped<TimerAction>();
            //Add suport  for MVC
            services.AddControllersWithViews();

            //Claims-based
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
                options.AddPolicy("DeletePie", policy => policy.RequireClaim("Delete Pie", "Delete Pie"));
                options.AddPolicy("AddPie", policy => policy.RequireClaim("Add Pie", "Add Pie"));
                options.AddPolicy("MinimumOrderAge", policy => policy.Requirements.Add(new MinimumOrderAgeRequirement(18)));
            });

            //Because identity uses Razor pages
            services.AddRazorPages();
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
            //NET CORE 2.0
            //  services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILoggerFactory loggerFactory,
             IHostApplicationLifetime lifetime, IDistributedCache cache)
        {

            // app.UseWelcomePage();

            lifetime.ApplicationStarted.Register(() =>
            {
                var currentTimeUTC = DateTime.UtcNow.ToString();
                byte[] encodedCurrentTimeUTC = Encoding.UTF8.GetBytes(currentTimeUTC);
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(20));
                cache.Set("cachedTimeUTC", encodedCurrentTimeUTC, options);
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }
            else
            {
                app.UseExceptionHandler("/AppException");
            }
         

            app.UseHttpsRedirection();

            app.UseResponseCompression();

            //By default will search in directoy wwwrot
            app.UseStaticFiles();
            //be sure it is before UseRouting!
            app.UseSession();
            //UseRouting and UseEndpoints enables to MVC handle incoming request
          
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            //loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());

            //Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "BethanysLogs-{Date}.txt"))
                .CreateLogger();

            loggerFactory.AddSerilog();

            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

        

            app.UseEndpoints(endpoints =>
            {
                //We don't want to respond to wvery request with hello world so we comment out this.
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});

                endpoints.MapControllerRoute(
                    name:"areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}"
                    );

                endpoints.MapControllerRoute(
                      name: "categoryfilter",
                    pattern: "Pie/{action}/{category?}",
                    defaults:new { Controller ="Pie", Action="List" }                   
                    );

                endpoints.MapControllerRoute(
                  name: "default",
                  pattern: "{controller=Home}/{action=Index}/{id?}"
                    );

                endpoints.MapRazorPages();
            });
        }
    }
}
