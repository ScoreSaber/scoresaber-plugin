#region

using System;
using System.Threading.Tasks;

#endregion

namespace ScoreSaber.Extensions {
    internal static class TaskEx {
        /// <summary>
        /// Blocks while condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block.</param>
        /// <param name="frequency">The frequency at which the condition will be check, in milliseconds.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        internal static async Task WaitWhile(Func<bool> condition, int frequency = 25, int timeout = -1) {

            var waitTask = Task.Run(async () => {
                while (condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
                throw new TimeoutException();
        }

        /// <summary>
        /// Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns></returns>
        internal static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1) {

            var waitTask = Task.Run(async () => {
                while (!condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout)))
                throw new TimeoutException();
        }

        /// <summary>Signifies that the argument is intentionally ignored.</summary>
        internal static void RunTask(this Task discarded) {

            discarded.ContinueWith(t => { Plugin.Log.Error(t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}