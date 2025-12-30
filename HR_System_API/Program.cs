using Microsoft.EntityFrameworkCore;
using HR_System_API.Models;
namespace HR_System_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // 註冊 EF Core 的 DbContext
            builder.Services.AddDbContext<HrContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            //加入 CORS 設定
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDev",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4200") // 允許 Angular 的網址
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAngularDev");
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
