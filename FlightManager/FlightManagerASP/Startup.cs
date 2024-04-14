using Data;
using Data.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightManagerASP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            CreateRole(serviceProvider).Wait();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<FmDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<dbUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;

            })
                .AddEntityFrameworkStores<FmDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        private async Task CreateRole(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<dbUser>>();
            string[] roles = { "Admin", "Employee" };
            IdentityResult result;

            //Checks if roles Admin and Employee exist. If not - they get created
            foreach (var role in roles)
            {
                var check = await RoleManager.RoleExistsAsync(role);
                if (!check)
                {
                    result = await RoleManager.CreateAsync(new IdentityRole(role));
                }
            }
            // Add admin user
            var admin = new dbUser
            {
                UserName = "Admin@admin.bg",
                FirstName = "Admin",
                LastName = "Admin",
                EGN = "0000000000",
                Address = "AdminNoAddress123",
                Email = "Admin@admin.bg",
                PhoneNumber = "1234567890",
                Id = Guid.NewGuid().ToString()
            };

            string passwordUser = "_Password123";

            //Checks if admin user exists. If it doesn't, it creates a admin user with role ,,Admin''
            var _user = await UserManager.FindByNameAsync(admin.UserName);
            if (_user == null)
            {
                IdentityResult checkUser = await UserManager.CreateAsync(admin, passwordUser);
                if (checkUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(admin, "Admin");
                }
            }
            
        }
    }
}
