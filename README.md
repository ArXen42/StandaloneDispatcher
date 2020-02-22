# Standalone .NET Core Dispatcher like class

This minimalistic library attempts to recreate basic functionality of .NET WPF [Dispatcher](https://docs.microsoft.com/en-us/dotnet/api/system.windows.threading.dispatcher?view=netframework-4.8) class.

`StandaloneDispatcher.IDispatcher` (with default implementation called `Dispatcher`) provides basic interface to run prioritized queue of user actions on single thread.

Original WPF Dispatcher is designed to only work as UI thread and, consequently, is much more complex than this implementation and only available under Windows.

The goal of this project is to provide simple and reliable way to push actions on a single thread under any .NET Standard environment.

It was developed with the following use case in mind: work with some native library that requires all communication with it be from single STA thread (for example, Canon EDSDK library).
However, you can use it whenever you need job queue running on a single thread.

# Implementation notes

Current implementation creates an instance of [ConcurrentQueue](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentqueue-1?view=netstandard-2.1) for each `DispatcherPriority` (currently there are 7) to hold dispatched delegates.

There is a single `SemaphoreSlim` used inside dispatcher loop to block loop execution until `InvokeAsync` methods are called.
Each `InvokeAsync` call produces instance of `DispatcherItem` that contains provided delegate and task completion source used to wait for execution result.

Because of the usage of `TaskCompletionSource` at the core of the library, there are no synchronous `Invoke` methods provided by `IDispatcher` as the only way to implement them is to simpy wait for resulting `Task` to complete.