namespace UNBUGGABLE.Commands;

public interface ICommand
{
    public string Name { get; }
    public void Execute();
    public void Undo();
}