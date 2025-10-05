namespace PowerPositionService.Worker.Utilities
{
    public static class RetryHelper
    {
        public static async Task<T> WithAttemptsAsync<T>(int attempts, Func<Task<T>> func, CancellationToken cancellationToken = default)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            if (attempts < 1) throw new ArgumentOutOfRangeException(nameof(attempts));

            int attempt = 1;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    return await func();
                }
                catch
                {
                    attempt++;
                    if (attempt > attempts) throw;
                }
            }
        }
    }
}
