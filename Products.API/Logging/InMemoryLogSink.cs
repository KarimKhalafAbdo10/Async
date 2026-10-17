using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Products.API.Logging;

public record LogEntry(long Id, DateTimeOffset Timestamp, string Level, string Source, string Message);

/// <summary>Serilog sink that keeps the latest events in memory so the UI can display them.</summary>
public class InMemoryLogSink : ILogEventSink
{
    private const int Capacity = 200;
    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private static readonly MessageTemplateTextFormatter Formatter = new("{Message:lj}");
    private long _nextId;

    public void Emit(LogEvent logEvent)
    {
        var writer = new StringWriter();
        Formatter.Format(logEvent, writer);
        var source = logEvent.Properties.TryGetValue("SourceContext", out var sc)
            ? sc.ToString().Trim('"').Split('.').Last()
            : "App";
        _entries.Enqueue(new LogEntry(Interlocked.Increment(ref _nextId), logEvent.Timestamp,
            logEvent.Level.ToString(), source, writer.ToString()));
        while (_entries.Count > Capacity) _entries.TryDequeue(out _);
    }

    public IReadOnlyList<LogEntry> GetAfter(long afterId) => _entries.Where(e => e.Id > afterId).ToList();
}
