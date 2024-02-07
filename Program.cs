var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSingleton<SqlConnectionFactory>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapClientEndpoints();

app.Run();
