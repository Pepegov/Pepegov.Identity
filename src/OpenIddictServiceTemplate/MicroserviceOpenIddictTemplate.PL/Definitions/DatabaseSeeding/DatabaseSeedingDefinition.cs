﻿using MicroserviceOpenIddictTemplate.DAL.Database;
using Microsoft.Extensions.DependencyInjection;
using Pepegov.MicroserviceFramework.Definition;
using Pepegov.MicroserviceFramework.Definition.Context;

namespace MicroserviceOpenIddictTemplate.PL.Definitions.DatabaseSeeding;

public class DatabaseSeedingDefinition : ApplicationDefinition
{
    public override async Task ConfigureApplicationAsync(IDefinitionApplicationContext context)
    {
        using var scope = context.ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>()!;

        await new DatabaseInitializer(context.ServiceProvider, dbContext).Seed();
    }
}