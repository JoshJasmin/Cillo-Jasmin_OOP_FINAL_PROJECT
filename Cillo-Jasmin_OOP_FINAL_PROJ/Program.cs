using Cillo_Jasmin_OOP_FINAL_PROJ.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dataFile = @"C:\Users\angel\OneDrive\Documents\GITHUB\Cillo-Jasmin_OOP_FINAL_PROJECT\Cillo-Jasmin_OOP_FINAL_PROJ\JsonAssetStorage.json";

var storage = new AssetFileStorage(dataFile);
var service = new AssetService(storage);

builder.Services.AddSingleton(storage);
builder.Services.AddSingleton(service);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();