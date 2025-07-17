namespace WorkflowEngine.Models;

public class WorkflowDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<State> States { get; set; } = new List<State>();
    public List<Action> Actions { get; set; } = new List<Action>();

    public bool Validate(out string error)
    {
        error = string.Empty;
        if (!States.Any(s => s.IsInitial))
        {
            error = "Workflow must have exactly one initial state.";
            return false;
        }
        if (States.Count(s => s.IsInitial) > 1)
        {
            error = "Workflow can have only one initial state.";
            return false;
        }
        if (States.Any(s => string.IsNullOrEmpty(s.Id)))
        {
            error = "All states must have a non-empty ID.";
            return false;
        }
        if (States.GroupBy(s => s.Id).Any(g => g.Count() > 1))
        {
            error = "State IDs must be unique.";
            return false;
        }
        if (Actions.Any(a => string.IsNullOrEmpty(a.Id)))
        {
            error = "All actions must have a non-empty ID.";
            return false;
        }
        if (Actions.GroupBy(a => a.Id).Any(g => g.Count() > 1))
        {
            error = "Action IDs must be unique.";
            return false;
        }
        if (Actions.Any(a => string.IsNullOrEmpty(a.ToState) || !States.Any(s => s.Id == a.ToState)))
        {
            error = "All actions must have a valid ToState.";
            return false;
        }
        if (Actions.Any(a => a.FromStates.Any(fs => string.IsNullOrEmpty(fs) || !States.Any(s => s.Id == fs))))
        {
            error = "All FromStates in actions must be valid.";
            return false;
        }
        return true;
    }
}