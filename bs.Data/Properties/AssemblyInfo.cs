using System.Runtime.CompilerServices;

// Exposes internal types (e.g. the deadlock retry policies in bs.Data.Helpers) to the test
// project so their behavior can be verified directly, without needing a live database
// connection to exercise the state-management logic (retry counters, back-off intervals).
[assembly: InternalsVisibleTo("bs.Data.TestAsync")]
