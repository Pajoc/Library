 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Library.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;//evita enviar formato padrão quando formato específico é pedido e não está disponível
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());//add nuget (Microsoft.AspNetCore.Mvc.Formatters.xml) na versão 2.1
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            });

            var connectionString = Startup.Configuration["connectionStrings:MyconnStr"];
            services.AddDbContext<LibraryContext>(o => {
                                                        o.UseSqlServer(connectionString,op => 
                                                        {
                                                            op.UseRowNumberForPaging();
                                                        });
                                                 });//por defeito scopped

            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddDebug(LogLevel.Information);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {   //Global exception handling
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        //para conseguir apanhar o erro antes de enviar a mensagem de erro genérica
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                        }
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }


            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Author, AuthorDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                src.DateOfBirth.GetCurrentAge()));

                cfg.CreateMap<Book, BookDto>();

                cfg.CreateMap<Models.AuthorForCreationDto, Author>();

                cfg.CreateMap<Models.BookForCreationDto, Book>();

                cfg.CreateMap<Models.BookForUpdateDto, Book>();

                cfg.CreateMap<Book, BookForUpdateDto>();

            });

            libraryContext.EnsureSeedDataForContext();

            app.UseStatusCodePages();
            app.UseMvc();
        }
    }
}
