using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.OpenApi.Models;
    using WorkflowEngine.Models;
    using WorkflowEngine.Services;

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container
    builder.Services.AddSingleton<WorkflowService>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Workflow Engine API", Version = "v1" });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow Engine API v1"));
    }

    app.UseHttpsRedirection();

    // Endpoints
    app.MapPost("/workflows", (WorkflowService service, WorkflowDefinition definition) =>
    {
        try
        {
            var result = service.CreateDefinition(definition);
            return Results.Created($"/workflows/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    });

    app.MapGet("/workflows/{id}", (WorkflowService service, string id) =>
    {
        var definition = service.GetDefinition(id);
        return definition != null ? Results.Ok(definition) : Results.NotFound();
    });

    app.MapGet("/workflows", (WorkflowService service) =>
    {
        return Results.Ok(service.ListDefinitions());
    });

    app.MapPost("/instances", (WorkflowService service, string definitionId) =>
    {
        try
        {
            var instance = service.StartInstance(definitionId);
            return Results.Created($"/instances/{instance.Id}", instance);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    });

    app.MapPost("/instances/{instanceId}/actions", (WorkflowService service, string instanceId, string actionId) =>
    {
        try
        {
            var instance = service.ExecuteAction(instanceId, actionId);
            return Results.Ok(instance);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    });

    app.MapGet("/instances/{id}", (WorkflowService service, string id) =>
    {
        var instance = service.GetInstance(id);
        return instance != null ? Results.Ok(instance) : Results.NotFound();
    });

    app.MapGet("/instances", (WorkflowService service) =>
    {
        return Results.Ok(service.ListInstances());
    });

    app.Run();
