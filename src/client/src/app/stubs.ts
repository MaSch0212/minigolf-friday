import { importProvidersFrom } from '@angular/core';
import {
  HttpClientEasyNetworkStub,
  HttpClientEasyNetworkStubModule,
  ErrorResponse,
} from '@ngneers/ng-httpclient-easy-network-stub';
import { Draft } from 'immer';
import { z } from 'zod';

import {
  AddEventRequest,
  AddEventResponse,
  AddPlayerToPreconfigRequest,
  AddPreconfigResponse,
  AddTimeSlotRequest,
  AddTimeSlotResponse,
  BuildInstancesResponse,
  EventTimeslot,
  GetAllEventsResponse,
  GetEventResponse,
  toApiEvent,
  toMinigolfEventTimeslot,
} from './models/api/event';
import { RegisterForEventRequest } from './models/api/player-event';
import { GetUsersByIdRequest } from './models/api/user';
import { MinigolfEvent, MinigolfEventInstance, MinigolfEventTimeslot } from './models/event';
import { AddMapResponse, GetMapsResponse } from './services/maps.service';
import { events, getId, maps, users } from './stub-data';
import { deepClone } from './utils/common.utils';

export function provideStubs() {
  return [
    importProvidersFrom(
      HttpClientEasyNetworkStubModule.forRoot({
        urlMatch: /\/api\//,
        stubFactory: configureStub,
      })
    ),
  ];
}

function configureStub(stub: HttpClientEasyNetworkStub) {
  // #region Users
  stub.stub('GET', 'administration/users', async () => {
    await defaultDelay();
    return respondWith(200, { users: users.map(deepClone) });
  });

  stub.stub2<GetUsersByIdRequest>()('POST', 'administration/users:by-ids', async ({ body }) => {
    await defaultDelay();
    const usersById = users.filter(p => body.userIds.includes(p.id));
    return respondWith(200, { users: usersById.map(deepClone) });
  });

  stub.stub('GET', 'administration/users/{id:string}', async ({ params }) => {
    await defaultDelay();
    const user = users.find(p => p.id === params.id);
    if (!user) {
      return respondWith(404);
    }
    return respondWith(200, { user: deepClone(user) });
  });
  // #endregion

  // #region Maps
  stub.stub('GET', 'administration/maps', async () => {
    await defaultDelay();
    return respondWith<GetMapsResponse>(200, { maps: maps.map(deepClone) });
  });

  stub.stub('POST', 'administration/maps', async ({ body }) => {
    await defaultDelay();
    const map = { ...body.map, id: getId() };
    maps.push(map);
    return respondWith<AddMapResponse>(201, { id: map.id });
  });

  stub.stub('PUT', 'administration/maps', async ({ body }) => {
    await defaultDelay();
    const mapIndex = maps.findIndex(p => p.id === body.map.id);
    if (mapIndex === -1) {
      return respondWith(404, { error: 'Map not found' });
    }
    maps[mapIndex] = body.map;
    return respondWith(200);
  });

  stub.stub('DELETE', 'administration/maps/{id:string}', async ({ params }) => {
    await defaultDelay();
    const mapIndex = maps.findIndex(p => p.id === params.id);
    if (mapIndex === -1) {
      return respondWith(404, { error: 'Map not found' });
    }
    maps.splice(mapIndex, 1);
    return respondWith(200);
  });
  // #endregion

  // #region Events
  stub.stub(
    'GET',
    'administration/events?page={page?:number}&pageSize={pageSize?:number}',
    async ({ params }) => {
      await defaultDelay();

      const page = params.page ?? 1;
      const pageSize = params.pageSize ?? 10;

      const start = (page - 1) * pageSize;
      const end = start + pageSize;

      return respondWith<z.infer<typeof GetAllEventsResponse>>(200, {
        events: events.slice(start, end).map(x => toApiEvent(deepClone(x))),
        totalAmount: events.length,
      });
    }
  );

  stub.stub('GET', 'administration/events/{id:string}', async ({ params }) => {
    await defaultDelay();
    const event = events.find(p => p.id === params.id);
    if (!event) {
      return respondWith(404);
    }
    return respondWith<z.infer<typeof GetEventResponse>>(200, {
      event: toApiEvent(deepClone(event)),
    });
  });

  stub.stub2<AddEventRequest>()('POST', 'administration/events', async ({ body }) => {
    await defaultDelay();
    const event: Draft<MinigolfEvent> = { ...body, id: getId(), timeslots: [] };
    events.push(event);
    return respondWith<z.infer<typeof AddEventResponse>>(201, {
      event: toApiEvent(deepClone(event)),
    });
  });

  stub.stub('DELETE', 'administration/events/{id:string}', async ({ params }) => {
    await defaultDelay();
    const eventIndex = events.findIndex(p => p.id === params.id);
    if (eventIndex === -1) {
      return respondWith(404, { error: 'Event not found' });
    }
    events.splice(eventIndex, 1);
    return respondWith(200);
  });

  stub.stub2<AddPlayerToPreconfigRequest>()(
    'POST',
    'administration/events:preconfigs/{preconfigId:string}/players',
    async ({ body, params }) => {
      await defaultDelay();
      const preconfig = events
        .flatMap(p => p.timeslots)
        .flatMap(p => p.preconfigurations)
        .find(p => p.id === params.preconfigId);
      if (!preconfig) {
        return respondWith(404, { error: 'Preconfig not found' });
      }
      preconfig.playerIds.push(body.playerId);
      return respondWith(200);
    }
  );

  stub.stub(
    'DELETE',
    'administration/events:preconfigs/{preconfigId:string}/players/{playerId:string}',
    async ({ params }) => {
      await defaultDelay();
      const preconfig = events
        .flatMap(p => p.timeslots)
        .flatMap(p => p.preconfigurations)
        .find(p => p.id === params.preconfigId);
      if (!preconfig) {
        return respondWith(404, { error: 'Preconfig not found' });
      }
      preconfig.playerIds = preconfig.playerIds.filter(p => p !== params.playerId);
      return respondWith(200);
    }
  );

  stub.stub2<AddTimeSlotRequest>()(
    'POST',
    'administration/events/{eventId:string}/timeslots',
    async ({ body, params }) => {
      await defaultDelay();
      const event = events.find(p => p.id === params.eventId);
      if (!event) {
        return respondWith(404, { error: 'Event not found' });
      }
      const timeslot: z.infer<typeof EventTimeslot> = {
        ...body,
        id: getId(),
        playerIds: [],
        preconfigurations: [],
      };
      event.timeslots.push(toMinigolfEventTimeslot(timeslot) as Draft<MinigolfEventTimeslot>);
      return respondWith<z.infer<typeof AddTimeSlotResponse>>(201, { timeslot });
    }
  );

  stub.stub('DELETE', 'administration/events:timeslots/{timeslotId:string}', async ({ params }) => {
    await defaultDelay();
    const timeslot = events.flatMap(p => p.timeslots).find(p => p.id === params.timeslotId);
    if (!timeslot) {
      return respondWith(404, { error: 'Timeslot not found' });
    }
    const event = events.find(p => p.timeslots.includes(timeslot));
    if (!event) {
      return respondWith(404, { error: 'Event not found' });
    }
    event.timeslots = event.timeslots.filter(p => p.id !== timeslot.id);
    return respondWith(200);
  });

  stub.stub(
    'POST',
    'administration/events:timeslots/{timeslotId:string}/preconfig',
    async ({ params }) => {
      await defaultDelay();
      const timeslot = events.flatMap(p => p.timeslots).find(p => p.id === params.timeslotId);
      if (!timeslot) {
        return respondWith(404, { error: 'Timeslot not found' });
      }
      const preconfig = { id: getId(), playerIds: [] };
      timeslot.preconfigurations.push(preconfig);
      return respondWith<z.infer<typeof AddPreconfigResponse>>(201, { preconfig });
    }
  );

  stub.stub(
    'DELETE',
    'administration/events:preconfigs/{preconfigId:string}',
    async ({ params }) => {
      await defaultDelay();
      const preconfig = events
        .flatMap(p => p.timeslots)
        .flatMap(p => p.preconfigurations)
        .find(p => p.id === params.preconfigId);
      if (!preconfig) {
        return respondWith(404, { error: 'Preconfig not found' });
      }
      const timeslot = events
        .flatMap(p => p.timeslots)
        .find(p => p.preconfigurations.includes(preconfig));
      if (!timeslot) {
        return respondWith(404, { error: 'Timeslot not found' });
      }
      timeslot.preconfigurations = timeslot.preconfigurations.filter(p => p.id !== preconfig.id);
      return respondWith(200);
    }
  );

  stub.stub(
    'POST',
    'administration/events/{eventId:string}/build-instances',
    async ({ params }) => {
      await defaultDelay();
      const event = events.find(p => p.id === params.eventId);
      if (!event) {
        return respondWith(404, { error: 'Event not found' });
      }
      const instances: { [timeslotId: string]: Draft<MinigolfEventInstance>[] } = {};
      for (const timeslot of event.timeslots) {
        instances[timeslot.id] = new Array(Math.ceil(timeslot.playerIds.length / 5)).map(() => {
          const id = getId();
          return <Draft<MinigolfEventInstance>>{ id, groupCode: `group${id}`, playerIds: [] };
        });
        for (let i = 0; i < instances[timeslot.id].length; i++) {
          instances[timeslot.id][i].playerIds = timeslot.playerIds.slice(i * 5, (i + 1) * 5);
        }
      }

      const persist = Date.now() >= event.registrationDeadline.getTime();
      if (persist) {
        Object.entries(instances).forEach(([timeslotId, instances]) => {
          const timeslot = event.timeslots.find(p => p.id === timeslotId);
          if (timeslot) {
            timeslot.instances = instances;
          }
        });
      }
      return respondWith<z.infer<typeof BuildInstancesResponse>>(200, {
        isPersisted: persist,
        instances,
      });
    }
  );

  stub.stub('GET', 'administration/events/{eventId:string}/instances', async ({ params }) => {
    await defaultDelay();
    const event = events.find(p => p.id === params.eventId);
    if (!event) {
      return respondWith(404, { error: 'Event not found' });
    }
    const instances: { [timeslotId: string]: Draft<MinigolfEventInstance>[] } = {};
    for (const timeslot of event.timeslots) {
      instances[timeslot.id] = timeslot.instances ?? [];
    }
    return respondWith(200, { instances });
  });
  // #endregion

  // #region Player Events
  stub.stub('GET', 'events', async ({ params }) => {});

  stub.stub('GET', 'events/{id:string}', async ({ params }) => {});

  stub.stub2<RegisterForEventRequest>()(
    'POST',
    'events/{id:string}/register',
    async ({ body, params }) => {}
  );
  // #endregion
}

function respondWith<T>(
  statusCode: number,
  content?: T,
  headers?: { [key: string]: string }
): never {
  throw <ErrorResponse<T>>{ statusCode, content, headers };
}

function defaultDelay() {
  return delay(100);
}
function delay(ms: number) {
  return new Promise(resolve => setTimeout(resolve, ms));
}
