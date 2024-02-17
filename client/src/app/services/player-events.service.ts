import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  GetPlayerEventResponse,
  GetPlayerEventsResponse,
  RegisterForEventRequest,
  toMinigolfPlayerEvent,
} from '../models/api/player-event';
import { mapUsingZod } from '../utils/rxjs.utils';

@Injectable({
  providedIn: 'root',
})
export class PlayerEventsService {
  private readonly _http = inject(HttpClient);

  public getEvents(params: {
    page?: number;
    pageSize?: number;
  }): Observable<GetPlayerEventsResponse> {
    return this._http.get('/api/events', { params }).pipe(
      mapUsingZod(GetPlayerEventsResponse, x => ({
        totalAmount: x.totalAmount,
        events: x.events.map(toMinigolfPlayerEvent),
      }))
    );
  }

  public getEvent(eventId: string): Observable<GetPlayerEventResponse> {
    return this._http
      .get(`/api/events/${eventId}`)
      .pipe(mapUsingZod(GetPlayerEventResponse, x => ({ event: toMinigolfPlayerEvent(x.event) })));
  }

  public registerForEvent(eventId: string, request: RegisterForEventRequest) {
    return this._http.post<void>(`/api/events/${eventId}/register`, request);
  }
}
