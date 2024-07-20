import { inject, Injectable } from '@angular/core';
import { defer, of, ReplaySubject, retry, switchMap, throwError, timer } from 'rxjs';

import { Logger } from './logger.service';
import { ApiGetWellKnownConfigurationResponse } from '../api/models';
import { WellKnownService as WellKnownApiService } from '../api/services';

@Injectable({ providedIn: 'root' })
export class WellKnownService {
  private readonly _api = inject(WellKnownApiService);

  public wellKnown$ = new ReplaySubject<ApiGetWellKnownConfigurationResponse>(1);

  constructor() {
    defer(() => this._api.getWellKnownConfiguration())
      .pipe(
        switchMap(response =>
          response.ok && response.body ? of(response.body) : throwError(() => response)
        ),
        retry({
          delay: error => {
            Logger.logError('WellKnownService', 'Error getting well-known configuration', {
              error,
            });
            return timer(5000);
          },
        })
      )
      .subscribe(this.wellKnown$);
  }
}
