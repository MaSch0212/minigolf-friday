import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { MinigolfMap } from '../models/minigolf-map';

export type GetMapsResponse = {
  maps: MinigolfMap[];
};
export type AddMapRequest = {
  map: MinigolfMap;
};
export type AddMapResponse = {
  id: string;
};
export type UpdateMapRequest = {
  map: MinigolfMap;
};

@Injectable({ providedIn: 'root' })
export class MapsService {
  private readonly _http = inject(HttpClient);

  public getMaps() {
    return this._http.get<GetMapsResponse>('/api/maps');
  }

  public addMap(map: MinigolfMap) {
    return this._http.post<AddMapResponse>('/api/maps', {
      map,
    } satisfies AddMapRequest);
  }

  public updateMap(map: MinigolfMap) {
    return this._http.put('/api/maps', { map } satisfies UpdateMapRequest);
  }

  public deleteMap(map: MinigolfMap) {
    return this._http.delete(`/api/maps/${map.id}`);
  }
}
