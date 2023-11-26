using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Service;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.RewardAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string serviceBusConnectionString;
    private readonly string orderCreatedTopic;
    private readonly string orderCreatedRewardsSubscription;

    private readonly IConfiguration _configuration;
    private readonly IRewardService _rewardService;

    private ServiceBusProcessor _rewardProcessor;

    public AzureServiceBusConsumer(IConfiguration configuration, IRewardService rewardService)
    {
        _configuration = configuration;
        _rewardService = rewardService;

        serviceBusConnectionString = _configuration.GetValue<string>("ServiceBus:ConnectionString");

        orderCreatedTopic = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        orderCreatedRewardsSubscription = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreated_Rewards_Subscription");

        var client = new ServiceBusClient(serviceBusConnectionString);
        _rewardProcessor = client.CreateProcessor(orderCreatedTopic, orderCreatedRewardsSubscription);
    }

    public async Task Start()
    {
        _rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
        _rewardProcessor.ProcessErrorAsync += ErrorHandler;
        await _rewardProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        await _rewardProcessor.StopProcessingAsync();
        await _rewardProcessor.DisposeAsync();
    }

    private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);

        RewardsMessage objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body);
        try
        {
            await _rewardService.UpdateRewards(objMessage);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}
