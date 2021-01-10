using Grpc.Core;
using Microsoft.Extensions.Logging;
using microservicesapp;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using paircalculator.Messaging;

namespace paircalculator
{
    public class PairCalculatorService : PairCalculator.PairCalculatorBase
    {
        private readonly ILogger<PairCalculatorService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IMessageQueueSender _mqSender;
        private readonly PairReply TRUE_RESULT = new PairReply { IsPair = true };
        private readonly PairReply FALSE_RESULT = new PairReply { IsPair = false };
        private readonly ValueTuple<bool, bool> CACHE_HIT_SUCCESS = new ValueTuple<bool, bool>(true, true);
        private readonly string _queueName;

        public PairCalculatorService(ILogger<PairCalculatorService> logger, IDistributedCache cache, IMessageQueueSender mqSender)
        {
            _logger = logger;
            _cache = cache;
            _mqSender = mqSender;
            _queueName = Constants.GetRabbitMQQueueName();
        }

        public override async Task<PairReply> IsItPair(PairRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                return FALSE_RESULT;
            }
            else if (request.Number % 2 == 0)
            {
                var answerFromCache = await GetFromCache(request.Number);
                if (answerFromCache == CACHE_HIT_SUCCESS) return TRUE_RESULT;
                await SetThePrimeInCache(request);
                _mqSender.Send(_queueName, request.Number.ToString());
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
