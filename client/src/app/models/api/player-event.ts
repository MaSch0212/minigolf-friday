import { z } from 'zod';

import { throwExp } from '../../utils/common.utils';
import { TIME_REGEX, parseTime } from '../../utils/date.utils';
import {
  MinigolfPlayerEvent,
  MinigolfPlayerEventInstance,
  MinigolfPlayerEventTimeslot,
} from '../player-event';

export const Map = z
  .object({
    id: z.string(),
    name: z.string(),
  })
  .readonly();

export const Player = z
  .object({
    id: z.string(),
    name: z.string(),
  })
  .readonly();

export const PlayerEventInstance = z
  .object({
    id: z.string(),
    groupCode: z.string(),
    map: Map,
    coPlayers: z.array(Player).readonly(),
  })
  .readonly();

export const PlayerEventTimeslot = z
  .object({
    id: z.string(),
    time: z.string().regex(TIME_REGEX),
    isFallbackAllowed: z.boolean(),
    isRegistered: z.boolean(),
    chosenFallbackTimeslotId: z.string().optional().nullable(),
    instance: PlayerEventInstance.optional().nullable(),
  })
  .readonly();

export const PlayerEvent = z
  .object({
    id: z.string(),
    date: z.string(),
    registrationDeadline: z.string(),
    timeslots: z.array(PlayerEventTimeslot).readonly(),
    isStarted: z.boolean(),
  })
  .readonly();

export const GetPlayerEventsResponse = z.object({
  events: z.array(PlayerEvent),
  totalAmount: z.number(),
});

export const GetPlayerEventResponse = z.object({
  event: PlayerEvent,
});

export type EventTimeslotRegistration = {
  timeslotId: string;
  fallbackTimeslotId?: string;
};
export type RegisterForEventRequest = {
  timeslotRegistrations: EventTimeslotRegistration[];
};

export type GetPlayerEventsResponse = { events: MinigolfPlayerEvent[]; totalAmount: number };
export type GetPlayerEventResponse = { event: MinigolfPlayerEvent };

export function toMinigolfPlayerEvent(event: z.infer<typeof PlayerEvent>): MinigolfPlayerEvent {
  return {
    id: event.id,
    date: new Date(event.date),
    registrationDeadline: new Date(event.registrationDeadline),
    timeslots: event.timeslots.map(toMinigolfPlayerEventTimeslot),
    isStarted: event.isStarted,
  };
}

function toMinigolfPlayerEventTimeslot(
  timeslot: z.infer<typeof PlayerEventTimeslot>
): MinigolfPlayerEventTimeslot {
  return {
    id: timeslot.id,
    time: parseTime(timeslot.time) ?? throwExp('Invalid time'),
    isFallbackAllowed: timeslot.isFallbackAllowed,
    isRegistered: timeslot.isRegistered,
    chosenFallbackTimeslotId: timeslot.chosenFallbackTimeslotId ?? undefined,
    instance: (timeslot.instance && toMinigolfPlayerEventInstance(timeslot.instance)) || undefined,
  };
}

function toMinigolfPlayerEventInstance(
  instance: z.infer<typeof PlayerEventInstance>
): MinigolfPlayerEventInstance {
  return {
    id: instance.id,
    groupCode: instance.groupCode,
    map: instance.map,
    coPlayers: instance.coPlayers,
  };
}
