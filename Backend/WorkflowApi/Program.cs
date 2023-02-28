using OptimaJet.Workflow.Core.Runtime;
using Microsoft.AspNetCore.SignalR;
using WorkflowApi;
using WorkflowApi.Hubs;
using WorkflowLib;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

const string rule = "MyCorsRule";
builder.Services.Configure<WorkflowApiConfiguration>(builder.Configuration);
var apiConfiguration = builder.Configuration.Get<WorkflowApiConfiguration>();

if (apiConfiguration?.Cors.Origins.Count > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(rule, policy =>
        {
            policy.WithOrigins(apiConfiguration.Cors.Origins.ToArray());
            policy.AllowAnyHeader();
            policy.AllowCredentials();
        });
    });
}

var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("Default");
if (connectionString is null) throw new NullReferenceException("Default connection string is not set");
await DatabaseUpgrade.WaitForUpgrade(connectionString);

WorkflowLib.WorkflowInit.ConnectionString = connectionString;

if (!string.IsNullOrEmpty(apiConfiguration?.LicenseKey))
{
    WorkflowRuntime.RegisterLicense(apiConfiguration.LicenseKey);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors(rule);

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ProcessConsoleHub>("api/workflow/processConsole");

var processConsoleContext = app.Services.GetService<IHubContext<ProcessConsoleHub>>();

await WorkflowInit.StartAsync(processConsoleContext);

app.Run();
