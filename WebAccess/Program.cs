
using WebAccess.Hubs;
using WebAccess.Service;
// using YourNamespace.Analysis; // ��� Analysis ���ⲿ��
// using YourNamespace.Camera;   // ��� Camera.Client ���ⲿ��

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ע����ķ���Ϊ����
builder.Services.AddSingleton<PostureDetectionService>();
// ������ Analysis �� Camera.Client �Ƕ������࣬������Ҫע��:
// builder.Services.AddSingleton<Analysis>();
// builder.Services.AddSingleton<Client>();


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