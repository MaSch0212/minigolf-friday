import { IRetryPolicy, RetryContext } from '@microsoft/signalr';

const RETRY_DELAYS = [0, 2000, 5000, 10000, 15000, 20000, 30000];

export class SignalrRetryPolicy implements IRetryPolicy {
  private readonly _onRetry: (error: Error, nextDelay: number) => void;

  constructor(onRetry: (error: Error, nextDelay: number) => void) {
    this._onRetry = onRetry;
  }

  public nextRetryDelayInMilliseconds(retryContext: RetryContext): number | null {
    const delay =
      retryContext.previousRetryCount < RETRY_DELAYS.length
        ? RETRY_DELAYS[retryContext.previousRetryCount]
        : RETRY_DELAYS[RETRY_DELAYS.length - 1];
    this._onRetry(retryContext.retryReason, delay);
    return delay;
  }
}
