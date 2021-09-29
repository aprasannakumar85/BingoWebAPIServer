using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using bingoWebAPI.Connection;
using FireSharp;
using FireSharp.Interfaces;

namespace bingoWebAPI
{
  public class Startup
  {
    private readonly IConfiguration _config;

    readonly string MyAllowSpecificOrigins = "AllowAll";

    public Startup(IConfiguration configuration)
    {
      _config = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "bingoWebAPI", Version = "v1" });
      });

      services.AddSingleton<IFirebaseClient>(sp =>
          new FirebaseClient(FireBaseConfigurationLoader.GetFireBaseConfiguration(_config)));

      services.AddCors(options =>
      {
        options.AddPolicy(MyAllowSpecificOrigins,
                  builder =>
                  {
                builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod().WithHeaders();
              });
      });
      services.AddSignalR();
      services.AddSingleton<ValidateConnection>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "bingoWebAPI v1"));
      }

      app.UseRouting();

      app.UseAuthorization();
      app.UseCors(MyAllowSpecificOrigins);

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapHub<BroadcastHub>("/notify");
      });

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
