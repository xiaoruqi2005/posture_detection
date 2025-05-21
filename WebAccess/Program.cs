
using WebAccess.Hubs;
using WebAccess.Service;
//using WebAccess.Respository
using Microsoft.Extensions.Configuration;
using WebAccess.Respoository;
// using YourNamespace.Analysis; // 如果 Analysis 是外部库
// using YourNamespace.Camera;   // 如果 Camera.Client 是外部库

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

// 注册核心服务为单例
builder.Services.AddSingleton<PostureDetectionService>();

 builder.Services.AddSingleton<PostureAnalysisRepository>();
/*string connectionString = builder.Configuration.GetConnectionString("MyDatabaseConnection")
                          ?? throw new InvalidOperationException("Connection string 'MyDatabaseConnection' not found in appsettings.json.");

builder.Services.AddScoped<PostureAnalysisRepository>(provider => // 构造函数接收 string connectionString
    new PostureAnalysisRepository(connectionString));
*/

// (重要) 配置 CORS，允许前端开发服务器访问
var frontendDevServerUrl = "http://localhost:5173"; // 你的 Vue/Vite 开发服务器地址
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins(frontendDevServerUrl)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // SignalR 需要
        });
});


var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection(); // 如果使用 HTTPS

// 应用 CORS 策略
app.UseCors("AllowVueApp");

app.UseRouting();

// app.UseAuthorization(); // 如果需要

app.MapControllers(); // 映射 API 控制器
app.MapHub<PostureHub>("/postureHub"); // 映射 SignalR Hub (路径与前端代理一致)

app.Run(); // 启动应用