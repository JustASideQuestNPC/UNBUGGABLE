namespace UNBUGGABLE.Commands;

public interface ICommand
{
    public string Name { get; }
    public bool UpdatesPriorityList { get; }
    public void Execute();
    public void Undo();
}