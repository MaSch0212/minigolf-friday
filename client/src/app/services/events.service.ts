import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

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
  GetInstancesResponse,
} from '../models/api/event';
import { MinigolfEventInstance } from '../models/event';
import { mapUsingZod } from '../utils/rxjs.utils';

@Injectable({
  providedIn: 'root',
})
export class EventsService {
  private readonly _http = inject(HttpClient);

  public getAllEvents(params: { page?: number; pageSize?: number }) {
    return this._http.get<GetAllEventsResponse>('/api/administration/events', { params }).pipe(
      mapUsingZod(GetAllEventsResponse, x => ({
        totalAmount: x.totalAmount,
        events: x.events.map(toMinigolfEvent),
      }))
    );
  }

  public addEvent(request: AddEventRequest): Observable<AddEventResponse> {
    return this._http
      .post('/api/administration/events', request)
      .pipe(mapUsingZod(AddEventResponse, x => ({ event: toMinigolfEvent(x.event) })));
  }

  public getEvent(id: string): Observable<GetEventResponse> {
    return this._http
      .get(`/api/administration/events/${id}`)
      .pipe(mapUsingZod(GetEventResponse, x => ({ event: toMinigolfEvent(x.event) })));
  }

  public removeEvent(id: string) {
    return this._http.delete<void>(`/api/administration/events/${id}`);
  }

  public addTimeSlot(
    eventId: string,
    request: AddTimeSlotRequest
  ): Observable<AddTimeSlotResponse> {
    return this._http
      .post(`/api/administration/events/${eventId}/timeslots`, request)
      .pipe(
        mapUsingZod(AddTimeSlotResponse, x => ({ timeslot: toMinigolfEventTimeslot(x.timeslot) }))
      );
  }

  public buildInstances(eventId: string): Observable<BuildInstancesResponse> {
    return this._http.post(`/api/administration/events/${eventId}/build-instances`, {}).pipe(
      mapUsingZod(BuildInstancesResponse, x => ({
        instances: Object.fromEntries(
          Object.entries(x.instances).map(([timeslotId, instances]) => [
            timeslotId,
            instances as MinigolfEventInstance[],
          ])
        ),
        isPersisted: x.isPersisted,
      }))
    );
  }

  public getInstances(eventId: string): Observable<GetInstancesResponse> {
    return this._http.get(`/api/administration/events/${eventId}/instances`).pipe(
      mapUsingZod(GetInstancesResponse, x => ({
        instances: Object.fromEntries(
          Object.entries(x.instances).map(([timeslotId, instances]) => [
            timeslotId,
            instances as MinigolfEventInstance[],
          ])
        ),
      }))
    );
  }

  public startEvent(eventId: string) {
    return this._http.post<void>(`/api/administration/events/${eventId}/start`, {});
  }

  public updateTimeslot(timeslotId: string, request: UpdateTimeslotRequest) {
    return this._http.patch<void>(`/api/administration/events:timeslots/${timeslotId}`, request);
  }

  public removeTimeslot(timeslotId: string) {
    return this._http.delete<void>(`/api/administration/events:timeslots/${timeslotId}`);
  }

  public addPreconfig(timeslotId: string): Observable<AddPreconfigResponse> {
    return this._http
      .post(`/api/administration/events:timeslots/${timeslotId}/preconfig`, {})
      .pipe(mapUsingZod(AddPreconfigResponse));
  }

  public removePreconfig(preconfigId: string) {
    return this._http.delete<void>(`/api/administration/events:preconfigs/${preconfigId}`);
  }

  public addPlayerToPreconfig(preconfigId: string, request: AddPlayerToPreconfigRequest) {
    return this._http.post<void>(
      `/api/administration/events:preconfigs/${preconfigId}/players`,
      request
    );
  }

  public removePlayerFromPreconfig(preconfigId: string, playerId: string) {
    return this._http.delete<void>(
      `/api/administration/events:preconfigs/${preconfigId}/players/${playerId}`
    );
  }
}
