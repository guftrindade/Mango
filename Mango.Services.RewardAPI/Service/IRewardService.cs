using Mango.Services.RewardAPI.Message;

namespace Mango.Services.RewardAPI.Service;

public interface IRewardService
{
    Task UpdateRewards(RewardsMessage rewardsMessage);
}
