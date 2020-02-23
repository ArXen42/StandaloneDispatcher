using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using StandaloneDispatcher.Exceptions;
using StandaloneDispatcher.Interfaces;
using Xunit;

namespace StandaloneDispatcher.Tests
{
	public class DispatcherTests
	{
		[Theory]
		[InlineData(DispatcherPriority.Lowest)]
		[InlineData(DispatcherPriority.Low)]
		[InlineData(DispatcherPriority.BelowNormal)]
		[InlineData(DispatcherPriority.Normal)]
		[InlineData(DispatcherPriority.AboveNormal)]
		[InlineData(DispatcherPriority.High)]
		[InlineData(DispatcherPriority.Highest)]
		public async Task SinglePriorityExecutionOrderIsFIFO(DispatcherPriority priority)
		{
			var dispatcher = new Dispatcher();
			var thread     = new Thread(dispatcher.Run);
			thread.Start();

			var numbers = new List<Int32>();
			var tasks   = new List<Task>();

			const Int32 jobsCount = 25;

			var rnd = new Random();
			for (Int32 i = 0; i < jobsCount; i++)
			{
				Int32 copy = i;
				tasks.Add(
					dispatcher.InvokeAsync(
						() =>
						{
							Thread.Sleep(rnd.Next(0, 50));
							numbers.Add(copy);
						},
						priority
					)
				);
			}

			await Task.WhenAll(tasks);
			numbers.Should().BeEquivalentTo(Enumerable.Range(0, jobsCount));
		}

		private static void Forget(Task t)
		{
		}

		[Fact]
		public async Task AllOperationsAreExecutedOnDispatcherThread()
		{
			using var dispatcher = new Dispatcher();
			var       thread     = new Thread(dispatcher.Run);
			thread.Start();

			void Action()
			{
				Thread.CurrentThread.Should().Be(thread);
			}

			void Func()
			{
				Thread.CurrentThread.Should().Be(thread);
			}

			var tasks = new List<Task>
			{
				dispatcher.InvokeAsync(Action),
				dispatcher.InvokeAsync(Func)
			};

			await Task.WhenAll(tasks);
		}

		[Fact]
		public async Task CanNotShutdownMultipleTimes()
		{
			using var dispatcher = new Dispatcher();
			var       thread     = new Thread(dispatcher.Run);
			thread.Start();

			await dispatcher.InvokeAsync(() => { }); // Await starting dispatcher

			Forget(dispatcher.InvokeAsync(() => Thread.Sleep(10))); // Add some tasks running
			Forget(dispatcher.InvokeAsync(() => Thread.Sleep(10))); // Add some tasks running
			Forget(dispatcher.InvokeAsync(() => Thread.Sleep(10))); // Add some tasks running

			const Int32 requestsCount = 100;
			const Int32 faultedCount  = requestsCount - 1;
			var         shutdownTasks = new Task[requestsCount];

			Parallel.For(
				0,
				requestsCount,
				i =>
				{
					// ReSharper disable once AccessToDisposedClosure
					shutdownTasks[i] = Task.Run(dispatcher.InvokeShutdownAsync);
				}
			);

			FluentActions.Awaiting(async () => await Task.WhenAll(shutdownTasks)).Should().Throw<DispatcherException>();
			shutdownTasks.Count(t => t.IsCompletedSuccessfully).Should().Be(1);
			shutdownTasks.Count(t => t.IsFaulted).Should().Be(faultedCount);
		}

		[Fact]
		public void ExceptionsAreForwarded()
		{
			using var dispatcher = new Dispatcher();
			var       thread     = new Thread(dispatcher.Run);
			thread.Start();

			FluentActions.Awaiting(() => dispatcher.InvokeAsync(() => throw new InvalidOperationException()))
			             .Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public async Task OperationsAreExecutedInPriorityOrder()
		{
			using var dispatcher = new Dispatcher();
			var       thread     = new Thread(dispatcher.Run);
			thread.Start();

			var rnd     = new Random();
			var numbers = new List<Int32>();
			var tasks   = new List<Task>();

			Int32 prioritiesCount = typeof(DispatcherPriority).GetEnumValues().Length;

			void Operation(DispatcherPriority priority)
			{
				Thread.Sleep(rnd.Next(0, 10));
				numbers.Add((Int32) priority);
			}

			tasks.Add(dispatcher.InvokeAsync(() => Thread.Sleep(200)));
			foreach (DispatcherPriority priority in typeof(DispatcherPriority).GetEnumValues())
				tasks.Add(dispatcher.InvokeAsync(() => Operation(priority)));

			await Task.WhenAll(tasks);
			numbers.Should().BeEquivalentTo(Enumerable.Range(0, prioritiesCount));
		}

		[Fact]
		public async Task ResultsAreForwarded()
		{
			using var dispatcher = new Dispatcher();
			var       thread     = new Thread(dispatcher.Run);
			thread.Start();

			const String expectedResult = "Test";
			String       result         = await dispatcher.InvokeAsync(() => expectedResult);
			result.Should().BeSameAs(expectedResult);
		}

		[Fact]
		public async Task ShutdownCancelsAllPendingJobs()
		{
			using var dispatcher = new Dispatcher();
			var       thread     = new Thread(dispatcher.Run);
			thread.Start();

			var firstTask    = dispatcher.InvokeAsync(() => Thread.Sleep(500));
			var pendingTasks = new List<Task>();
			for (Int32 i = 0; i < 100; i++)
				pendingTasks.Add(dispatcher.InvokeAsync(() => { }));

			await dispatcher.InvokeShutdownAsync();

			firstTask.Status.Should().Be(TaskStatus.RanToCompletion);

			foreach (var task in pendingTasks)
				FluentActions.Awaiting(() => task).Should().Throw<OperationCanceledException>();
		}
	}
}