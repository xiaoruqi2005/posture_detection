
using WebAccess.Hubs;
using WebAccess.Service;
//using WebAccess.Respository
using Microsoft.Extensions.Configuration;
using WebAccess.Respoository;
// using YourNamespace.Analysis; // ��� Analysis ���ⲿ��
// using YourNamespace.Camera;   // ��� Camera.Client ���ⲿ��

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ע����ķ���Ϊ����
builder.Services.AddSingleton<PostureDetectionService>();

 builder.Services.AddSingleton<PostureAnalysisRepository>();
/*string connectionString = builder.Configuration.GetConnectionString("MyDatabaseConnection")
                          ?? throw new InvalidOperationException("Connection string 'MyDatabaseConnection' not found in appsettings.json.");

builder.Services.AddScoped<PostureAnalysisRepository>(provider => // ���캯������ string connectionString
    new PostureAnalysisRepository(connectionString));
*/

// (��Ҫ) ���� CORS������ǰ�˿�������������
var frontendDevServerUrl = "http://localhost:5173"; // ��� Vue/Vite ������������ַ
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins(frontendDevServerUrl)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // SignalR ��Ҫ
        });
});


var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection(); // ���ʹ�� HTTPS

// Ӧ�� CORS ����
app.UseCors("AllowVueApp");

app.UseRouting();

// app.UseAuthorization(); // �����Ҫ

app.MapControllers(); // ӳ�� API ������
app.MapHub<PostureHub>("/postureHub"); // ӳ�� SignalR Hub (·����ǰ�˴���һ��)

app.Run(); // ����Ӧ��