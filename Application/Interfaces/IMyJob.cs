using Quartz;

namespace Application.Interfaces;

public interface IMyJob
{
    public Task NewJob<T>(long id,string time,IScheduler scheduler) where T : IJob;

    public Task StopJob(long id, IScheduler scheduler);
}