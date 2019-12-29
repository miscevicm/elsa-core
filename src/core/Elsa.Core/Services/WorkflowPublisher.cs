using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Elsa.Models;
using Elsa.Persistence;

namespace Elsa.Services
{
    public class WorkflowPublisher : IWorkflowPublisher
    {
        private readonly IWorkflowDefinitionStore store;
        private readonly IIdGenerator idGenerator;
        private readonly IMapper mapper;

        public WorkflowPublisher(
            IWorkflowDefinitionStore store,
            IIdGenerator idGenerator,
            IMapper mapper)
        {
            this.store = store;
            this.idGenerator = idGenerator;
            this.mapper = mapper;
        }

        public ProcessDefinitionVersion New()
        {
            var definition = new ProcessDefinitionVersion
            {
                Id = idGenerator.Generate(),
                DefinitionId = idGenerator.Generate(),
                Name = "New Workflow",
                Version = 1,
                IsLatest = true,
                IsPublished = false,
                IsSingleton = false,
                IsDisabled = false
            };

            return definition;
        }

        public async Task<ProcessDefinitionVersion> PublishAsync(
            string id,
            CancellationToken cancellationToken)
        {
            var definition = await store.GetByIdAsync(id, VersionOptions.Latest, cancellationToken);

            if (definition == null)
                return null;

            return await PublishAsync(definition, cancellationToken);
        }

        public async Task<ProcessDefinitionVersion> PublishAsync(
            ProcessDefinitionVersion processDefinition,
            CancellationToken cancellationToken)
        {
            var definition = mapper.Map<ProcessDefinitionVersion>(processDefinition);

            var publishedDefinition = await store.GetByIdAsync(
                definition.DefinitionId,
                VersionOptions.Published,
                cancellationToken);

            if (publishedDefinition != null)
            {
                publishedDefinition.IsPublished = false;
                publishedDefinition.IsLatest = false;
                await store.UpdateAsync(publishedDefinition, cancellationToken);
            }

            if (definition.IsPublished)
            {
                definition.Id = idGenerator.Generate();
                definition.Version++;
            }
            else
            {
                definition.IsPublished = true;   
            }

            definition.IsLatest = true;
            definition = Initialize(definition);
            
            await store.SaveAsync(definition, cancellationToken);

            return definition;
        }

        public async Task<ProcessDefinitionVersion> GetDraftAsync(
            string id,
            CancellationToken cancellationToken)
        {
            var definition = await store.GetByIdAsync(id, VersionOptions.Latest, cancellationToken);

            if (definition == null)
                return null;

            if (!definition.IsPublished)
                return definition;

            var draft = mapper.Map<ProcessDefinitionVersion>(definition);
            draft.Id = idGenerator.Generate();
            draft.IsPublished = false;
            draft.IsLatest = true;
            draft.Version++;

            return draft;
        }

        public async Task<ProcessDefinitionVersion> SaveDraftAsync(
            ProcessDefinitionVersion processDefinition,
            CancellationToken cancellationToken)
        {
            var draft = mapper.Map<ProcessDefinitionVersion>(processDefinition);
            
            var latestVersion = await store.GetByIdAsync(
                processDefinition.DefinitionId,
                VersionOptions.Latest,
                cancellationToken);

            if (latestVersion != null && latestVersion.IsPublished && latestVersion.IsLatest)
            {
                latestVersion.IsLatest = false;
                draft.Id = idGenerator.Generate();
                draft.Version++;
                
                await store.UpdateAsync(latestVersion, cancellationToken);
            }
   
            draft.IsLatest = true;
            draft.IsPublished = false;
            draft = Initialize(draft);
            
            await store.SaveAsync(draft, cancellationToken);

            return draft;
        }

        private ProcessDefinitionVersion Initialize(ProcessDefinitionVersion processDefinition)
        {
            if (processDefinition.Id == null)
                processDefinition.Id = idGenerator.Generate();

            if (processDefinition.Version == 0)
                processDefinition.Version = 1;

            if (processDefinition.DefinitionId == null)
                processDefinition.DefinitionId = idGenerator.Generate();

            return processDefinition;
        }
    }
}