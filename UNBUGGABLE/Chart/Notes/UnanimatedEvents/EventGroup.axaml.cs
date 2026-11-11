using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace UNBUGGABLE.Notes.UnanimatedEvents;

public class EventGroupListEntry(EventGroupDialog parentDialog, UnanimatedEventBase e)
{
    public UnanimatedEventBase Event = e;
    public string Name => Event.EventType.Name();

    public void RemoveEvent()
    {
        parentDialog.RemoveEvent(this);
    }

    public async void EditEvent()
    {
        await parentDialog.EditEvent(this);
    }
}

public partial class EventGroupDialog : UnanimatedEventBaseDialog
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    public bool GroupChanged { get; private set; }
    public List<UnanimatedEventBase> Events => _listEntries.Select(e => e.Event).ToList();

    private readonly ObservableCollection<EventGroupListEntry> _listEntries = [];

    private long _startTime;
    
    public EventGroupDialog(long startTime, List<UnanimatedEventBase> events)
    {
        InitializeComponent();
        _startTime = startTime;

        foreach (var e in events)
        {
            _listEntries.Add(new EventGroupListEntry(this, e));
        }
        
        EventList.ItemsSource = _listEntries;
    }

    public async Task EditEvent(EventGroupListEntry e)
    {
        var edited = await e.Event.ShowEditDialog();
        if (edited != null)
        {
            e.Event = edited;
            GroupChanged = true;
        }
    }

    public void RemoveEvent(EventGroupListEntry e)
    {
        _listEntries.Remove(e);
        GroupChanged = true;
    }

    private async void AddEventButtonClick(object? sender, RoutedEventArgs e)
    {
        var eventType = await new ChooseEventTypeDialog(false).ShowAsync();
        if (eventType != null && eventType is { HasValue: true, Value: not null })
        {
            var eventClass = ((EventType)eventType.Value).NoteClass();
            if (eventClass == null)
            {
                return;
            }
                
            Logger.Debug("Creating unanimated event: {0}", eventType);
                
            // create a dummy instance of the note to spawn the edit dialog and generate the
            // actual note
            if (Activator.CreateInstance(eventClass) is UnanimatedEventBase eventInstance)
            {
                eventInstance.Time = _startTime;
                var configured = await eventInstance.ShowEditDialog(true);
                if (configured != null)
                {
                    _listEntries.Add(new EventGroupListEntry(this, configured));
                    GroupChanged = true;
                }
            }
        }
    }
}

public class EventGroup : UnanimatedEventBase
{
    public List<UnanimatedEventBase> Events { get; set; } = [];
    
    public override EventType EventType => EventType.EVENT_GROUP;

    public override async Task<UnanimatedEventBase?> ShowEditDialog(bool isNew = false)
    {
        var editDialog = new EventGroupDialog(Time, Events);
        var result = await editDialog.ShowAsync();
        if (result is { HasValue: true, Value: true } && (editDialog.GroupChanged || isNew))
        {
            return new EventGroup
            {
                Time = Time,
                EndTime = EndTime,
                Events = editDialog.Events
            };
        }
        
        return null;
    }

    public override string ToEventString()
    {
        List<string> lines = [];
        foreach (var e in Events)
        {
            lines.Add(e.ToEventString());
        }

        return string.Join('\n', lines);
    }
}