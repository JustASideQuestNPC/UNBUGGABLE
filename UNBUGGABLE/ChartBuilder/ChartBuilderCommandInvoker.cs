using System.Collections.Generic;
using System.Linq;
using UNBUGGABLE.Commands;

namespace UNBUGGABLE;

public class CommandInvokerDebugInfo
{
    public required List<string> UndoStackNames;
    public required List<string> RedoStackNames;
}

public static class ChartBuilderCommandInvoker
{
    public static CommandInvokerDebugInfo DebugInfo => new()
    {
        UndoStackNames = CommandStack.ToList().Select(c => c.Name).ToList(),
        RedoStackNames = RedoStack.ToList().Select(c => c.Name).ToList()
    };
    
    private static readonly Stack<ICommand> CommandStack = new();
    private static readonly Stack<ICommand> RedoStack = new();

    public static void Reset()
    {
        CommandStack.Clear();
        RedoStack.Clear();
    }
    
    public static void Execute(ICommand command)
    {
        CommandStack.Push(command);
        RedoStack.Clear();
        command.Execute();
        if (command.UpdatesPriorityList)
        {
            App.MainWindowViewModel.UpdatePriorityListEntries();
        }
    }
    
    public static void Undo()
    {
        if (CommandStack.Count == 0)
        {
            return;
        }
        
        var command = CommandStack.Pop();
        command.Undo();
        RedoStack.Push(command);
        if (command.UpdatesPriorityList)
        {
            App.MainWindowViewModel.UpdatePriorityListEntries();
        }
    }
    
    public static void Redo()
    {
        if (RedoStack.Count == 0)
        {
            return;
        }
        
        var command = RedoStack.Pop();
        command.Execute();
        CommandStack.Push(command);
        if (command.UpdatesPriorityList)
        {
            App.MainWindowViewModel.UpdatePriorityListEntries();
        }
    }
}