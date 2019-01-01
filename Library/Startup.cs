using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library
{
    
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
               
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var connectionString = Startup.Configuration["connectionStrings:MyconnStr"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));//por defeito scopped

            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, LibraryContext libraryContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Author, AuthorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                src.DateOfBirth.GetCurrentAge()));
            });

            //libraryContext.EnsureSeedDataForContext();

            app.UseStatusCodePages();
            app.UseMvc();
        }
    }
}
