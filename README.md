# Power Position Service

A Windows Service built with .NET 9 that periodically retrieves power trade data, aggregates hourly positions, and exports the results as CSV files.

## Tech Stack

- .NET 9 
- Worker Service template  
- xUnit for unit tests

## Design Considerations
- **Easily configurable**: Interval and output directory can be set in `appsettings.json`.
```json
"Scheduler": {
  "IntervalMinutes": 5,
  "OutputDirectory": "C:/Exports/PowerPositions"
}
```

- **Robust scheduling**: Uses `PeriodicTimer` to trigger each extraction cycle. Each cycle runs asyncronously to ensure that a cycle taking longer than the interval doesn't block the next cycle from starting so extractions are not missed. The call to the `PowerService` is retried up to 10 times to ensure that transient errors don't cause an extraction to fail.
- **Loose coupling**: Data retrieval, aggregation, and export services are represented as interfaces, allowing these to be swapped out easily for unit testing or future changes.
- **Error handling**: Exceptions during data retrieval, aggregation, or export are caught and logged without crashing the service.

## Future Improvements
- **Improve unit testability of Worker**: Currently, the `Worker` class is tightly coupled with `PeriodicTimer`, making it difficult to unit test. I could refactor this to allow injection of a timer interface for better testability.