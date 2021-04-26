# DateTimeExtensions

Extension methods for `System.DateTime` that provide common date/time manipulations and formatting utilities.

## API

### `IsPast(DateTime date)`
Determines whether the given `date` is in the past relative to `DateTime.UtcNow`.

- **Returns**: `true` if `date` is earlier than the current UTC time; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `IsFuture(DateTime date)`
Determines whether the given `date` is in the future relative to `DateTime.UtcNow`.

- **Returns**: `true` if `date` is later than the current UTC time; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `ElapsedSince(DateTime date)`
Calculates the elapsed time between the given `date` and the current UTC time.

- **Returns**: A `TimeSpan` representing the duration between `date` and now.
- **Throws**: Does not throw exceptions.

### `ToIso8601(DateTime date)`
Formats the given `date` as an ISO 8601 string in UTC.

- **Returns**: A string in ISO 8601 format (e.g., `"2023-10-05T14:30:00Z"`).
- **Throws**: Does not throw exceptions.

### `ToRelativeFormat(DateTime date)`
Formats the given `date` as a human-readable relative string (e.g., "2 hours ago", "in 3 days").

- **Returns**: A localized relative time string.
- **Throws**: Does not throw exceptions.

### `StartOfDay(DateTime date)`
Truncates the given `date` to the start of its day (midnight).

- **Returns**: A `DateTime` representing the start of the day (time component set to `00:00:00`).
- **Throws**: Does not throw exceptions.

### `EndOfDay(DateTime date)`
Truncates the given `date` to the end of its day (one tick before midnight of the next day).

- **Returns**: A `DateTime` representing the end of the day (time component set to `23:59:59.9999999`).
- **Throws**: Does not throw exceptions.

### `StartOfMonth(DateTime date)`
Truncates the given `date` to the first day of its month at midnight.

- **Returns**: A `DateTime` representing the start of the month.
- **Throws**: Does not throw exceptions.

### `EndOfMonth(DateTime date)`
Truncates the given `date` to the last day of its month at the end of the day.

- **Returns**: A `DateTime` representing the end of the month.
- **Throws**: Does not throw exceptions.

### `IsBetween(DateTime date, DateTime start, DateTime end)`
Determines whether the given `date` falls between `start` and `end` (inclusive).

- **Parameters**:
  - `date`: The date to check.
  - `start`: The start of the range.
  - `end`: The end of the range.
- **Returns**: `true` if `date` is between `start` and `end` (inclusive); otherwise, `false`.
- **Throws**: Does not throw exceptions.

## Usage
