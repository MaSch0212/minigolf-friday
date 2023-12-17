import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';

export type WellKnown = {
  facebookAppId: string;
};

@Injectable({ providedIn: 'root' })
export class WellKnownService {
  private readonly _http = inject(HttpClient);
  private _wellKnown$?: Observable<WellKnown>;

  public getWellKnown() {
    if (!this._wellKnown$) {
      this._wellKnown$ = this._http.get<WellKnown>('/api/.well-known').pipe(shareReplay(1));
    }
    return this._wellKnown$;
  }
}
