import { IRetryPolicy } from '@microsoft/signalr';
import { retry, throwError, timer } from 'rxjs';

export function retryWithPolicy(policy: IRetryPolicy) {
  return retry({
    delay: (error, retryCount) => {
      const nextDelay = policy.nextRetryDelayInMilliseconds({
        elapsedMilliseconds: 0,
        previousRetryCount: retryCount,
        retryReason: error,
      });
      return nextDelay === null ? throwError(() => error) : timer(nextDelay);
    },
  });
}
