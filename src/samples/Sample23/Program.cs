using System;
using System.Threading.Tasks;
using Elsa.Activities.Console.Extensions;
using Elsa.Extensions;
using Elsa.Persistence;
using Elsa.Persistence.EntityFrameworkCore.CustomSchema;
using Elsa.Persistence.EntityFrameworkCore.DbContexts;
using Elsa.Persistence.EntityFrameworkCore.Extensions;
using Elsa.Runtime;
using Elsa.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sample23
{
    /// <summary>
    /// A simple demonstration of using Entity Framework Core persistence providers.
    /// To run the EF migration, first run the following command: `dotnet ef database update`.
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = BuildServices();

            // Invoke startup tasks.
            var startupRunner = services.GetRequiredService<IStartupRunner>();
            await startupRunner.StartupAsync();

            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ElsaContext>();

            // Ensure DB exists.
            await dbContext.Database.EnsureCreatedAsync();

            // Create a workflow definition.
            var registry = services.GetService<IWorkflowRegistry>();
            var workflowDefinition = await registry.GetWorkflowDefinitionAsync<HelloWorldWorkflow>();

            // Mark this definition as the "latest" version.
            workflowDefinition.IsLatest = true;
            workflowDefinition.Version = 1;

            // Execute the workflow.
            var runner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
            var executionContext = await runner.RunAsync(workflowDefinition);

            // Persist the workflow instance.
            var instanceStore = scope.ServiceProvider.GetRequiredService<IWorkflowInstanceStore>();
            var workflowInstance = executionContext.ProcessInstance.ToInstance();
            await instanceStore.SaveAsync(workflowInstance);

            // Flush to DB.
            await dbContext.SaveChangesAsync();
        }

        private static IServiceProvider BuildServices()
        {
            return new ServiceCollection()
                .AddElsa(
                    elsaBuilder =>
                        elsaBuilder.AddCustomSchema("elsa")
                        .AddEntityFrameworkStores(
                            options => options                                                           
                                .UseSqlite(@"Data Source=c:\data\elsa.entity-framework-core.db;Cache=Shared",
                                x =>
                                {
                                    x.AddCustomSchemaModelSupport(options, elsaBuilder.Services);
                                    x.MigrationsAssembly(typeof(Program).Assembly.FullName);
                                    x.MigrationsHistoryTableWithSchema(options);
                                 })))
                .AddStartupRunner()
                .AddConsoleActivities()
                .AddWorkflow<HelloWorldWorkflow>()
                .BuildServiceProvider();
        }
    }
}