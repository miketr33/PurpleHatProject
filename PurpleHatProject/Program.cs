using Amazon.DynamoDBv2;
using Microsoft.EntityFrameworkCore;
using PurpleHatProject.Components;
using PurpleHatProject.Data;
using PurpleHatProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var dynamoDbServiceUrl = builder.Configuration["DynamoDb:ServiceUrl"];
if (!string.IsNullOrEmpty(dynamoDbServiceUrl))
{
    builder.Services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient(
        new AmazonDynamoDBConfig { ServiceURL = dynamoDbServiceUrl }));
}
else
{
    builder.Services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient());
}

builder.Services.AddSingleton<ITrackService, TrackService>();
builder.Services.AddScoped<IUserSessionService, UserSessionService>();
builder.Services.AddScoped<ITrackFavouriteService, TrackFavouriteService>();
builder.Services.AddScoped<IPlaybackStateService, PlaybackStateService>();
builder.Services.AddSingleton<IPlaybackSessionService, PlaybackSessionService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();