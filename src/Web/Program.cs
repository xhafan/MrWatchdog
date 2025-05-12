using Rebus.Config;
using Rebus.Transport.InMem;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvcBuilder = builder.Services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
    });
}

builder.Services.AddRebus(configure => configure
    .Transport(x =>
    {
        x.UseInMemoryTransport(new InMemNetwork(), $"{builder.Environment.EnvironmentName}Web", registerSubscriptionStorage: false);
    })    
    .Options(x =>
    {
        x.SetNumberOfWorkers(0); // no worker for unused Web queue
        x.SetMaxParallelism(1); // must be 1 to make Rebus happy
    })
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
