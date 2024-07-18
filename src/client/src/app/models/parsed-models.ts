import {
  ApiEvent,
  ApiEventInstance,
  ApiEventInstancePreconfiguration,
  ApiEventTimeslot,
  ApiMinigolfMap,
  ApiPlayerEvent,
  ApiPlayerEventTimeslot,
  ApiPlayerPreferences,
  ApiRole,
  ApiUser,
  ApiUserSettings,
} from '../api/models';
import { throwExp } from '../utils/common.utils';
import { parseTime, Time } from '../utils/date.utils';
import { ChangePropertyTypes } from '../utils/type.utils';

export type Role = ApiRole;
export type User = ApiUser & { loginToken?: string };
export type PlayerPreferences = ApiPlayerPreferences;

export type MinigolfMap = ApiMinigolfMap;

export type Event = ChangePropertyTypes<
  ApiEvent,
  {
    date: Date;
    registrationDeadline: Date;
    timeslots: EventTimeslot[];
    startedAt: Date;
    staged: boolean;
  }
>;
export type EventTimeslot = ChangePropertyTypes<
  ApiEventTimeslot,
  {
    time: Time;
  }
>;
export type EventInstancePreconfiguration = ApiEventInstancePreconfiguration;
export type EventInstance = ApiEventInstance;

export type PlayerEvent = ChangePropertyTypes<
  ApiPlayerEvent,
  {
    date: Date;
    registrationDeadline: Date;
    timeslots: PlayerEventTimeslot[];
  }
>;
export type PlayerEventTimeslot = ChangePropertyTypes<
  ApiPlayerEventTimeslot,
  {
    time: Time;
  }
>;

export type UserSettings = ApiUserSettings;

export function parseEvent(event: ApiEvent): Event {
  return {
    ...event,
    date: new Date(event.date),
    registrationDeadline: new Date(event.registrationDeadline),
    timeslots: event.timeslots.map(parseEventTimeslot),
    startedAt: event.startedAt ? new Date(event.startedAt) : undefined,
    staged: event.staged != null ? event.staged : undefined,
  };
}

export function parseEventTimeslot(timeslot: ApiEventTimeslot): EventTimeslot {
  return {
    ...timeslot,
    time: parseTime(timeslot.time) ?? throwExp(`Invalid time: ${timeslot.time}`),
  };
}

export function parsePlayerEvent(event: ApiPlayerEvent): PlayerEvent {
  return {
    ...event,
    date: new Date(event.date),
    registrationDeadline: new Date(event.registrationDeadline),
    timeslots: event.timeslots.map(parsePlayerEventTimeslot),
  };
}

export function parsePlayerEventTimeslot(timeslot: ApiPlayerEventTimeslot): PlayerEventTimeslot {
  return {
    ...timeslot,
    time: parseTime(timeslot.time) ?? throwExp(`Invalid time: ${timeslot.time}`),
  };
}
