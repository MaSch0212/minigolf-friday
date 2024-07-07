import { effect, inject, Injectable } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { SwUpdate } from '@angular/service-worker';
import { merge, filter, map, debounceTime } from 'rxjs';

import { Logger } from './logger.service';
import { RealtimeEventsService } from './realtime-events.service';
import { onDocumentVisibilityChange$ } from '../utils/event.utils';

@Injectable({ providedIn: 'root' })
export class UpdateService {
  private readonly _swUpdate = inject(SwUpdate);
  private readonly _realtimeEventsService = inject(RealtimeEventsService);

  public readonly newVersionAvailable = toSignal(
    this._swUpdate.versionUpdates.pipe(
      filter(x => x.type === 'VERSION_READY'),
      map(() => true)
    ),
    { initialValue: false }
  );

  constructor() {
    if (this._swUpdate.isEnabled) {
      merge(
        onDocumentVisibilityChange$().pipe(filter(isVisible => isVisible)),
        this._realtimeEventsService.onReconnected$
      )
        .pipe(takeUntilDestroyed(), debounceTime(1000))
        .subscribe(async () => {
          Logger.logDebug('UpdateService', 'Checking for updates...');
          const result = await this._swUpdate.checkForUpdate();
          Logger.logDebug('UpdateService', 'Update check result:', result);
        });

      this._swUpdate.unrecoverable.pipe(takeUntilDestroyed()).subscribe(() => {
        Logger.logError(
          'UpdateService',
          'Unrecoverable error in service worker, reloading page...'
        );
        location.reload();
      });

      this._swUpdate.versionUpdates.pipe(takeUntilDestroyed()).subscribe(x => {
        Logger.logDebug('UpdateService', 'Got version update event', x);
      });
    }

    effect(() => {
      if (this.newVersionAvailable()) {
        Logger.logInfo('UpdateService', 'New version available');
      }
    });
  }
}
