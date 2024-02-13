import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import {
  GetAllEventsResponse,
  toMinigolfEvent,
  AddEventRequest,
  AddEventResponse,
  GetEventResponse,
  AddTimeSlotRequest,
  AddTimeSlotResponse,
  toMinigolfEventTimeslot,
  BuildInstancesResponse,
  UpdateTimeslotRequest,
  AddPreconfigResponse,
  AddPlayerToPreconfigRequest,
} from '../models/api/event';
import { MinigolfEventInstance } from '../models/event';
import { mapUsingZod } from '../utils/rxjs.utils';

@Injectable({
  providedIn: 'root',
})
export class EventsService {
  private readonly _http = inject(HttpClient);

  public getAllEvents(params: { page?: number; pageSize?: number }) {
    return this._http.get<GetAllEventsResponse>('/api/events', { params }).pipe(
      mapUsingZod(GetAllEventsResponse, x => ({
        totalAmount: x.totalAmount,
        events: x.events.map(toMinigolfEvent),
      }))
    );
  }

  public addEvent(request: AddEventRequest) {
    return this._http
      .post<AddEventResponse>('/api/events', request)
      .pipe(mapUsingZod(AddEventResponse, x => ({ event: toMinigolfEvent(x.event) })));
  }

  public getEvent(id: string) {
    return this._http
      .get<GetEventResponse>(`/api/events/${id}`)
      .pipe(mapUsingZod(GetEventResponse, x => ({ event: toMinigolfEvent(x.event) })));
  }

  public addTimeSlot(eventId: string, request: AddTimeSlotRequest) {
    return this._http
      .post<AddTimeSlotResponse>(`/api/events/${eventId}/timeslots`, request)
      .pipe(
        mapUsingZod(AddTimeSlotResponse, x => ({ timeslot: toMinigolfEventTimeslot(x.timeslot) }))
      );
  }

  public buildInstances(eventId: string) {
    return this._http
      .post<BuildInstancesResponse>(`/api/events/${eventId}/build-instances`, {})
      .pipe(
        mapUsingZod(BuildInstancesResponse, x => ({
          instances: Object.fromEntries(
            Object.entries(x.instances).map(([timeslotId, instances]) => [
              timeslotId,
              instances as MinigolfEventInstance[],
            ])
          ),
        }))
      );
  }

  public updateTimeslot(timeslotId: string, request: UpdateTimeslotRequest) {
    return this._http.put<void>(`/api/events:timeslots/${timeslotId}`, request);
  }

  public addPreconfig(timeslotId: string) {
    return this._http
      .post<AddPreconfigResponse>(`/api/events:timeslots/${timeslotId}/preconfig`, {})
      .pipe(mapUsingZod(AddPreconfigResponse));
  }

  public removePreconfig(preconfigId: string) {
    return this._http.delete<void>(`/api/events:preconfigs/${preconfigId}`);
  }

  public addPlayerToPreconfig(preconfigId: string, request: AddPlayerToPreconfigRequest) {
    return this._http.post<void>(`/api/events:preconfigs/${preconfigId}/players`, request);
  }

  public removePlayerFromPreconfig(preconfigId: string, playerId: string) {
    return this._http.delete<void>(`/api/events:preconfigs/${preconfigId}/players/${playerId}`);
  }
}
