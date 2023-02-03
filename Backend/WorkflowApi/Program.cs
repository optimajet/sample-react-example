using WorkflowApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

const string rule = "MyCorsRule";
builder.Services.Configure<WorkflowApiConfiguration>(builder.Configuration);
var apiConfiguration = builder.Configuration.Get<WorkflowApiConfiguration>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(rule, policy =>
    {
        policy.WithOrigins(apiConfiguration.Cors.Origins.ToArray());
    });
});

var app = builder.Build();


var connectionString = app.Configuration.GetConnectionString("Default");
if (connectionString is null) throw new NullReferenceException("Default connection string is not set");
await WorkflowApi.DatabaseUpgrade.WaitForUpgrade(connectionString);

WorkflowLib.WorkflowInit.ConnectionString = connectionString;

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

app.Run();
