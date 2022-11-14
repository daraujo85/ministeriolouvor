using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinisterioLouvor.Context;
using MinisterioLouvor.Interfaces;
using MinisterioLouvor.Persistence;
using MinisterioLouvor.Respository;

namespace MinisterioLouvor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        [System.Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure the persistence in another layer
            MongoDbPersistence.Configure();
                
            RegisterServices(services);

            RegisterSwagger(services);

            services.AddControllers();

            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");                                
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(option => option.AllowAnyOrigin());

            app.UseAuthorization();

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IMongoContext, MongoContext>();
            services.AddScoped<IMusicaRepository, MusicaRepository>();
            services.AddScoped<ICifraRepository, CifraRepository>();
        }

        private void RegisterSwagger(IServiceCollection services)
        {
            // Set the comments path for the Swagger JSON and UI.**
            //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            // Register the Swagger generator, defining 1 or more Swagger documents
            //services.AddSwaggerGen(c => c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true));
            services.AddSwaggerGen();
        }
    }
}
