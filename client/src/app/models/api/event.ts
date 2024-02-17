import { z } from 'zod';

import { throwExp } from '../../utils/common.utils';
import { TIME_REGEX, parseTime, timeToString } from '../../utils/date.utils';
import {
  MinigolfEvent,
  MinigolfEventInstance,
  MinigolfEventInstancePreconfiguration,
  MinigolfEventTimeslot,
} from '../event';

export const EventInstancePreconfiguration = z
  .object({
    id: z.string(),
    playerIds: z.array(z.string()).readonly(),
  })
  .readonly();

export const EventInstance = z
  .object({
    id: z.string(),
    groupCode: z.string(),
    playerIds: z.array(z.string()).readonly(),
  })
  .readonly();

export const EventTimeslot = z
  .object({
    id: z.string(),
    time: z.string().regex(TIME_REGEX),
    mapId: z.string(),
    isFallbackAllowed: z.boolean(),
    preconfigurations: z.array(EventInstancePreconfiguration).readonly(),
    playerIds: z.array(z.string()).readonly(),
    instances: z.array(EventInstance).readonly(),
  })
  .readonly();

export const Event = z
  .object({
    id: z.string(),
    date: z.string(),
    registrationDeadline: z.string(),
    timeslots: z.array(EventTimeslot).readonly(),
    isStarted: z.boolean(),
  })
  .readonly();

export const GetAllEventsResponse = z.object({
  events: z.array(Event),
  totalAmount: z.number(),
});

export const AddEventResponse = z.object({
  event: Event,
});

export const GetEventResponse = z.object({
  event: Event,
});

export const AddTimeSlotResponse = z.object({
  timeslot: EventTimeslot,
});

export const BuildInstancesResponse = z.object({
  instances: z.record(z.array(EventInstance)),
  isPersisted: z.boolean(),
});

export const GetInstancesResponse = z.object({
  instances: z.record(z.array(EventInstance)),
});

export const AddPreconfigResponse = z.object({
  preconfig: EventInstancePreconfiguration,
});

export type GetAllEventsResponse = { events: MinigolfEvent[]; totalAmount: number };
export type AddEventResponse = { event: MinigolfEvent };
export type GetEventResponse = { event: MinigolfEvent };
export type AddTimeSlotResponse = { timeslot: MinigolfEventTimeslot };
export type BuildInstancesResponse = {
  instances: Record<string, MinigolfEventInstance[]>;
  isPersisted: boolean;
};
export type GetInstancesResponse = { instances: Record<string, MinigolfEventInstance[]> };
export type AddPreconfigResponse = { preconfig: MinigolfEventInstancePreconfiguration };

export type AddEventRequest = { date: string; registrationDeadline: Date };
export type AddTimeSlotRequest = { time: string; mapId: string; isFallbackAllowed: boolean };
export type UpdateTimeslotRequest = { mapId?: string; isFallbackAllowed?: boolean };
export type AddPlayerToPreconfigRequest = { playerId: string };

export function toMinigolfEvent(event: z.infer<typeof Event>): MinigolfEvent {
  return {
    id: event.id,
    date: new Date(event.date),
    registrationDeadline: new Date(event.registrationDeadline),
    timeslots: event.timeslots.map(toMinigolfEventTimeslot),
    isStarted: event.isStarted,
  };
}

export function toMinigolfEventTimeslot(
  timeslot: z.infer<typeof EventTimeslot>
): MinigolfEventTimeslot {
  return {
    id: timeslot.id,
    time: parseTime(timeslot.time) ?? throwExp('Invalid time'),
    mapId: timeslot.mapId,
    isFallbackAllowed: timeslot.isFallbackAllowed,
    preconfigurations: timeslot.preconfigurations,
    playerIds: timeslot.playerIds,
    instances: timeslot.instances,
  };
}

export function toApiEvent(event: MinigolfEvent): z.infer<typeof Event> {
  return {
    id: event.id,
    date: event.date instanceof Date ? event.date.toISOString() : event.date,
    registrationDeadline:
      event.registrationDeadline instanceof Date
        ? event.registrationDeadline.toISOString()
        : event.registrationDeadline,
    timeslots: event.timeslots.map(toApiEventTimeslot),
    isStarted: event.isStarted,
  };
}

export function toApiEventTimeslot(timeslot: MinigolfEventTimeslot): z.infer<typeof EventTimeslot> {
  return {
    id: timeslot.id,
    time: timeToString(timeslot.time),
    mapId: timeslot.mapId,
    isFallbackAllowed: timeslot.isFallbackAllowed,
    preconfigurations: timeslot.preconfigurations,
    playerIds: timeslot.playerIds,
    instances: timeslot.instances,
  };
}
