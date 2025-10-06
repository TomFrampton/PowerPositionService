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

                    await Task.Delay(TimeSpan.FromSeconds(2 * (attempt - 1)), cancellationToken);
                }
            }
        }

        public static async Task WithAttemptsAsync(int attempts, Func<Task> func, CancellationToken cancellationToken = default)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            await WithAttemptsAsync(attempts, async () =>
            {
                await func();
                return true;

            }, cancellationToken);
        }
    }
}
