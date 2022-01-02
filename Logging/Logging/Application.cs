using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Logging.Enums;
using Logging.Services;
using Logging.Services.Abstractions;

namespace Logging
{
    public class Application
    {
        private readonly ILogger _logger;
        private readonly IBackupServices _backupServices;
        public Application(
            ILogger logger,
            IBackupServices backupServices)
        {
            _logger = logger;
            _backupServices = backupServices;
        }

        public async Task Run()
        {
            _logger.OnBackedUp += _backupServices.Create;

            var tcs1 = new TaskCompletionSource();
            var tcs2 = new TaskCompletionSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await RunRandomLogs(1, 51, tcs1));
            Task.Run(async () => await RunRandomLogs(1001, 1051, tcs2));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await tcs1.Task;
            await tcs2.Task;

            await _logger.DisposeAsync();
        }

        private async Task RunRandomLogs(int start, int end, TaskCompletionSource tcs)
        {
            const int logTypesAmount = 2;
            var rand = new Random();
            for (var i = start; i < end; i++)
            {
                var message = i.ToString();
                var randomLogType = (LogType)rand.Next(logTypesAmount);
                switch (randomLogType)
                {
                    case LogType.Error:
                        await _logger.LogError(message);
                        break;
                    case LogType.Info:
                        await _logger.LogInfo(message);
                        break;
                }
            }

            tcs.SetResult();
        }
    }
}