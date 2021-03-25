# ValidationResult

A lightweight utility class used to collect and report validation messages (errors and warnings) during source generation or runtime validation scenarios. It provides methods to aggregate messages and format them for diagnostics or user feedback.

## API

### `public bool IsValid`
Gets a value indicating whether there are no errors. Returns `true` if the `Errors` list is empty; otherwise, `false`.

### `public List<string> Errors`
Gets the collection of error messages. This list is mutable and can be modified directly or via `AddError`.

### `public List<string> Warnings`
Gets the collection of warning messages. This list is mutable and can be modified directly or via `AddWarning`.

### `public void AddError(string message)`
Adds an error message to the `Errors` list. The `message` parameter must not be `null`; otherwise, an `ArgumentNullException` is thrown.

### `public void AddWarning(string message)`
Adds a warning message to the `Warnings` list. The `message` parameter must not be `null`; otherwise, an `ArgumentNullException` is thrown.

### `public IEnumerable<string> GetAllMessages()`
Returns an enumerable sequence of all messages, combining errors and warnings in the order: errors first, followed by warnings. Each message is prefixed with `"Error: "` or `"Warning: "` respectively.

### `public override string ToString()`
Returns a formatted string representation of all messages, with each message on a new line. Errors appear before warnings, each prefixed appropriately. Returns an empty string if no messages exist.

## Usage
