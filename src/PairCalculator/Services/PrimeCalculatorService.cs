using Grpc.Core;
using Microsoft.Extensions.Logging;
using microservicesapp;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace paircalculator
{
    public class PairCalculatorService : PairCalculator.PairCalculatorBase
    {
        private readonly ILogger<PairCalculatorService> _logger;
        private readonly IDistributedCache _cache;
        private readonly PairReply TRUE_RESULT = new PairReply { IsPair = true };
        private readonly PairReply FALSE_RESULT = new PairReply { IsPair = false };
        private readonly ValueTuple<bool, bool> CACHE_HIT_SUCCESS = new ValueTuple<bool, bool>(true, true);

        public PairCalculatorService(ILogger<PairCalculatorService> logger, IDistributedCache cache)
        {
            _logger = logger;
            _cache = cache;
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
                var answerFromCache = await GetFromCache(request.Number);
                if (answerFromCache == CACHE_HIT_SUCCESS) return TRUE_RESULT;
                return TRUE_RESULT;
            }
            else
            {
                return FALSE_RESULT;
            }
        }

        private async Task<ValueTuple<bool, bool>> GetFromCache(long input)
        {
            var answerHit = await _cache.GetStringAsync(input.ToString());
            if (answerHit == null) return new ValueTuple<bool, bool>(false, false);

            return new ValueTuple<bool, bool>(true, true);
        }

        private async Task SetThePrimeInCache(PairRequest request)
        {
            _logger.LogInformation("Setting the pair number in the cache: " + request.Number);

            await _cache.SetStringAsync(request.Number.ToString(), "true", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
        }

    }
}
