using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Example.OpenIdConnect.Models;
using Microsoft.Data.Entity;
using Microsoft.AspNet.Authentication;
using Microsoft.Extensions.Logging;
using Example.OpenIdConnect.Extensions;
using Microsoft.AspNet.Authentication.Cookies;
using Example.OpenIdConnect.Providers;
using System.Security.Cryptography;
using System.IdentityModel.Tokens;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Example.OpenIdConnect
{
    public class Startup
    {


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<ApplicationContext>(options => {
                    options.UseInMemoryDatabase();
                })
                .AddDbContext<MembershipContext>(options =>
                {
                    options.UseInMemoryDatabase();
                });

            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MembershipContext>()
                .AddDefaultTokenProviders();

            services.Configure<SharedAuthenticationOptions>(options => {
                options.SignInScheme = "ServerCookie";
            });

            services.AddAuthentication();
            services.AddCaching();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment hostEnv, ILoggerFactory loggerFactory)
        {
            app.UseIISPlatformHandler();

            app.UseDeveloperExceptionPage();

            var factory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            factory.AddConsole();

            app.UseCookieAuthentication(options => {
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
                options.AuthenticationScheme = "ServerCookie";
                options.CookieName = CookieAuthenticationDefaults.CookiePrefix + "ServerCookie";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = new PathString("/signin");
            });

            app.UseOpenIdConnectServer(options => {
                options.Provider = new AuthorizationProvider();

                // Note: see AuthorizationController.cs for more
                // information concerning ApplicationCanDisplayErrors.
                options.ApplicationCanDisplayErrors = true;
                options.AllowInsecureHttp = true;

                // Note: by default, tokens are signed using dynamically-generated
                // RSA keys but you can also use your own certificate:
                // options.SigningCredentials.AddCertificate(certificate);
            });

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseMvc();

            app.UseWelcomePage();

            using (var database = app.ApplicationServices.GetService<ApplicationContext>())
            {
                database.Applications.Add(new Application
                {
                    ApplicationID = "API",
                    DisplayName = "API",
                    RedirectUri = "http://localhost:59627/signin-oidc",
                    LogoutRedirectUri = "http://localhost:59627/",
                    Secret = "secret_secret_secret"
                });
                database.Applications.Add(new Application
                {
                    ApplicationID = "Gateway",
                    DisplayName = "Gateway",
                    RedirectUri = "http://localhost:59871/signin-oidc",
                    LogoutRedirectUri = "http://localhost:59871/",
                    Secret = "secret_secret_secret"
                });
                database.Applications.Add(new Application
                {
                    ApplicationID = "Module",
                    DisplayName = "Module",
                    RedirectUri = "http://localhost:59882/signin-oidc",
                    LogoutRedirectUri = "http://localhost:59882/",
                    Secret = "secret_secret_secret"
                });

                database.SaveChanges();
            }

            var user_manager = app.ApplicationServices.GetService<Microsoft.AspNet.Identity.UserManager<ApplicationUser>>();
            var user = new ApplicationUser()
            {
                UserName = "test",
                Email = "test@test.com",
            };
            var result = user_manager.CreateAsync(user, "T3s5_90");
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        private static RSAParameters GenerateRsaKeys()
        {
            var myRSA = new RSACryptoServiceProvider(2048);
            return myRSA.ExportParameters(true);
        }
    }
}
