import { inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { SwUpdate } from '@angular/service-worker';
import { merge, filter, map } from 'rxjs';

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
      ).subscribe(() => {
        console.info('Checking for updates...');
        this._swUpdate.checkForUpdate().then(x => console.info('Update check result:', x));
      });
      this._swUpdate.unrecoverable.subscribe(() => location.reload());
    }
  }
}
