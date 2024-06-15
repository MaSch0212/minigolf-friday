import { inject, Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';

import { ApiGetWellKnownConfigurationResponse } from '../api/models';
import { WellKnownService as WellKnownApiService } from '../api/services';
import { assertBody } from '../utils/http.utils';

@Injectable({ providedIn: 'root' })
export class WellKnownService {
  private readonly _api = inject(WellKnownApiService);

  public wellKnown$ = new ReplaySubject<ApiGetWellKnownConfigurationResponse>(1);

  constructor() {
    this._api.getWellKnownConfiguration().then(response => {
      if (response.ok) {
        this.wellKnown$.next(assertBody(response));
      }
      this.wellKnown$.error(response);
    });
  }
}
