# MiddlewarePipeline

A lightweight pipeline for chaining middleware components in a composable way. It supports both synchronous and asynchronous middleware execution, allowing fine-grained control over request processing flow.

## API

### `public MiddlewarePipeline`

Initializes a new instance of the `MiddlewarePipeline` class.

### `public IMiddlewarePipeline Use<TMiddleware>()`

Adds a middleware component of type `TMiddleware` to the pipeline.

- **Parameters**: None.
- **Return value**: `IMiddlewarePipeline` – The pipeline instance for method chaining.
- **Throws**: `ArgumentException` – If `TMiddleware` does not implement `IMiddleware`.

### `public IMiddlewarePipeline Use(DelegateMiddleware middleware)`

Adds a delegate-based middleware to the pipeline.

- **Parameters**:
  - `middleware` – A delegate of type `DelegateMiddleware` representing the middleware logic.
- **Return value**: `IMiddlewarePipeline` – The pipeline instance for method chaining.
- **Throws**: `ArgumentNullException` – If `middleware` is `null`.

### `public IMiddlewarePipeline Use(IMiddleware middleware)`

Adds an existing `IMiddleware` instance to the pipeline.

- **Parameters**:
  - `middleware` – An instance of a type implementing `IMiddleware`.
- **Return value**: `IMiddlewarePipeline` – The pipeline instance for method chaining.
- **Throws**: `ArgumentNullException` – If `middleware` is `null`.

### `public async Task ExecuteAsync(object? context)`

Executes the entire middleware pipeline asynchronously with the given context.

- **Parameters**:
  - `context` – An optional context object passed through each middleware.
- **Return value**: `Task` – A task representing the asynchronous execution.
- **Throws**: `InvalidOperationException` – If the pipeline is empty or if middleware execution fails.

### `public DelegateMiddleware`

A delegate type representing a middleware component. Defined as:
`public delegate Task DelegateMiddleware(object? context, Func<Task> next);`

- **Parameters**:
  - `context` – The context object passed through the pipeline.
  - `next` – A continuation delegate that invokes the next middleware in the chain.

### `public Task InvokeAsync(object? context)`

Invokes the pipeline with the given context asynchronously.

- **Parameters**:
  - `context` – An optional context object passed through each middleware.
- **Return value**: `Task` – A task representing the asynchronous invocation.
- **Throws**: `InvalidOperationException` – If the pipeline is empty or if middleware execution fails.

## Usage
