using System.Collections.Generic;
using UNBUGGABLE.Commands;

namespace UNBUGGABLE;

public static class CommandInvoker
{
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
    }
}