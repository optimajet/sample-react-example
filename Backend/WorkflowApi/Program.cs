using OptimaJet.Workflow.Core.Runtime;
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

builder.Services.AddSingleton<MsSqlProviderLocator>();
builder.Services.AddSingleton<IWorkflowActionProvider,ActionProvider>();
builder.Services.AddSingleton<IWorkflowRuleProvider,SimpleRuleProvider>();
builder.Services.AddSingleton<IDesignerParameterFormatProvider,DesignerParameterFormatProvider>();
builder.Services.AddSingleton<WorkflowRuntimeLocator>();

var app = builder.Build();

var connectionString = app.Configuration.GetConnectionString("Default");
if (connectionString is null) throw new NullReferenceException("Default connection string is not set");
await DatabaseUpgrade.WaitForUpgrade(connectionString);

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

await app.Services.GetService<WorkflowRuntimeLocator>().Runtime.StartAsync();

app.Run();
