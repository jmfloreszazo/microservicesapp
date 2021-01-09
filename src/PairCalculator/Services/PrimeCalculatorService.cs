using Grpc.Core;
using Microsoft.Extensions.Logging;
using microservicesapp;
using System;
using System.Threading.Tasks;

namespace paircalculator
{
    public class PairCalculatorService : PairCalculator.PairCalculatorBase
    {
        private readonly ILogger<PairCalculatorService> _logger;

        private readonly PairReply TRUE_RESULT = new PairReply { IsPair = true };
        private readonly PairReply FALSE_RESULT = new PairReply { IsPair = false };

        public PairCalculatorService(ILogger<PairCalculatorService> logger)
        {
            _logger = logger;
        }

        public override async Task<PairReply> IsItPair(PairRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                return FALSE_RESULT;
            }
            else if (request.Number % 2 == 0)
            {
                return TRUE_RESULT;
            }
            else
            {
                return FALSE_RESULT;
            }
        }
    }
}
