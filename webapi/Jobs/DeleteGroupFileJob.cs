using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Jobs
{
    [DisallowConcurrentExecution]
    public class DeleteGroupFileJob : IJob
    {

        public Task Execute(IJobExecutionContext context)
        {
            // this operation job

            return Task.CompletedTask;
        }
    }
}
