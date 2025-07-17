namespace WorkflowEngine.Services;

    using WorkflowEngine.Models;

    public class WorkflowService
    {
        private readonly List<WorkflowDefinition> _definitions = new();
        private readonly List<WorkflowInstance> _instances = new();

        public WorkflowDefinition CreateDefinition(WorkflowDefinition definition)
        {
            if (!definition.Validate(out string error))
                throw new ArgumentException(error);
            _definitions.Add(definition);
            return definition;
        }

        public WorkflowDefinition? GetDefinition(string id)
        {
            return _definitions.FirstOrDefault(d => d.Id == id);
        }

        public WorkflowInstance StartInstance(string definitionId)
        {
            var definition = _definitions.FirstOrDefault(d => d.Id == definitionId);
            if (definition == null)
                throw new ArgumentException("Workflow definition not found.");
            if (!definition.Validate(out string error))
                throw new ArgumentException(error);

            var initialState = definition.States.First(s => s.IsInitial);
            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = definitionId,
                CurrentStateId = initialState.Id
            };
            _instances.Add(instance);
            return instance;
        }

        public WorkflowInstance ExecuteAction(string instanceId, string actionId)
        {
            var instance = _instances.FirstOrDefault(i => i.Id == instanceId);
            if (instance == null)
                throw new ArgumentException("Workflow instance not found.");

            var definition = _definitions.FirstOrDefault(d => d.Id == instance.WorkflowDefinitionId);
            if (definition == null)
                throw new ArgumentException("Workflow definition not found.");

            var currentState = definition.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
            if (currentState == null || !currentState.Enabled)
                throw new ArgumentException("Current state is invalid or disabled.");

            if (currentState.IsFinal)
                throw new ArgumentException("Cannot execute actions on a final state.");

            var action = definition.Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null || !action.Enabled)
                throw new ArgumentException("Action not found or disabled.");

            if (!action.FromStates.Contains(instance.CurrentStateId))
                throw new ArgumentException("Action not valid from current state.");

            instance.CurrentStateId = action.ToState;
            instance.History.Add(new HistoryEntry { ActionId = actionId, Timestamp = DateTime.UtcNow });
            return instance;
        }

        public WorkflowInstance? GetInstance(string id)
        {
            return _instances.FirstOrDefault(i => i.Id == id);
        }

        public List<WorkflowDefinition> ListDefinitions()
        {
            return _definitions;
        }

        public List<WorkflowInstance> ListInstances()
        {
            return _instances;
        }
    }