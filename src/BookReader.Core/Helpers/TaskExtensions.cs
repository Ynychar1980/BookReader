using BookReader.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookReader.Core.Helpers;

public static class TaskExtensions
{
    public static void SafeFireAndForget(this Task task, bool logError = true)
    {
        task.ContinueWith(t =>
        {
            if (t.IsFaulted && logError)
                LoggerService.LogError("Async error", t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
