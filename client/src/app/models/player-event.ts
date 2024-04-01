import { MinigolfMap } from './minigolf-map';
import { Time } from '../utils/date.utils';

export type MinigolfPlayerEvent = {
  readonly id: string;
  readonly date: Date;
  readonly registrationDeadline: Date;
  readonly timeslots: readonly MinigolfPlayerEventTimeslot[];
  readonly isStarted: boolean;
};

export type MinigolfPlayerEventTimeslot = {
  readonly id: string;
  readonly time: Time;
  readonly isFallbackAllowed: boolean;
  readonly isRegistered: boolean;
  readonly chosenFallbackTimeslotId?: string;
  readonly instance?: MinigolfPlayerEventInstance;
};

export type Player = {
  readonly id: string;
  readonly name: string;
};

export type MinigolfPlayerEventInstance = {
  readonly id: string;
  readonly groupCode: string;
  readonly map: MinigolfMap;
};
