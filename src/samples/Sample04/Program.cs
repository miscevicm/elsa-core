using System;
using System.Data;
using System.Threading.Tasks;
using Elsa.Activities.Console.Extensions;
using Elsa.Extensions;
using Elsa.Models;
using Elsa.Services;
using Elsa.Services.Extensions;
using Elsa.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Sample04.Activities;

namespace Sample04
{
    /// <summary>
    /// A strongly-typed, long-running workflows program demonstrating scripting, branching and resuming suspended workflows by providing user driven stimuli.
    /// </summary>
    internal static class Program
    {
        private static async Task Main()
        {
            // Setup a service collection.
            var services = new ServiceCollection()
                .AddElsa()
                .AddConsoleActivities()
                .AddActivity<Sum>()
                .AddActivity<Subtract>()
                .AddActivity<Multiply>()
                .AddActivity<Divide>()
                .BuildServiceProvider();

            // Create a workflow.
            var workflowFactory = services.GetRequiredService<IWorkflowFactory>();
            var workflow = workflowFactory.CreateWorkflow<CalculatorWorkflow>();

            // Run the workflow.
            var runner = services.GetService<IWorkflowRunner>();
            var executionContext = await runner.RunAsync(workflow);

            // Keep resuming the workflow until it completes.
            while (executionContext.ProcessInstance.Status != ProcessStatus.Completed)
            {
                // Print current execution log + blocking activities to visualize current workflow state.
                DisplayWorkflowState(executionContext.ProcessInstance);
                
                var textInput = Console.ReadLine();
                var input = Variable.From(textInput);

                executionContext.ProcessInstance.Input = input;
                executionContext = await runner.ResumeAsync(executionContext.ProcessInstance, executionContext.ProcessInstance.BlockingActivities);
            }

            Console.WriteLine("Workflow has ended. Here are the activities that have executed:");
            DisplayWorkflowState(executionContext.ProcessInstance);

            Console.ReadLine();
        }

        private static void DisplayWorkflowState(Workflow workflow)
        {
            var table = GetExecutionLogDataTable(workflow);
            table.Print();
        }

        private static DataTable GetExecutionLogDataTable(Workflow workflow)
        {
            var workflowDefinitionVersion = workflow.Blueprint;
            var table = new DataTable { TableName = workflowDefinitionVersion.Name };

            table.Columns.Add("Id", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Faulted", typeof(bool));
            table.Columns.Add("Blocking", typeof(bool));
            table.Columns.Add("Message", typeof(string));
            table.Columns.Add("Timestamp", typeof(DateTime));

            foreach (var entry in workflow.ExecutionLog)
            {
                var activity = workflow.GetActivity(entry.ActivityId);
                var activityDefinition = workflowDefinitionVersion.GetActivity(activity.Id);

                table.Rows.Add(activity.Id, activity.Type, activityDefinition.DisplayName, activityDefinition.Description, entry.Faulted, false, entry.Message, entry.Timestamp.ToDateTimeUtc());
            }

            foreach (var activity in workflow.BlockingActivities)
            {
                var activityDefinition = workflowDefinitionVersion.GetActivity(activity.Id);
                table.Rows.Add(activity.Id, activity.Type, activityDefinition.DisplayName, activityDefinition.Description, false, true, "Waiting for input...", null);
            }

            return table;
        }
    }
}