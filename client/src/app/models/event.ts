import { Time } from '../utils/date.utils';

export type MinigolfEvent = {
  readonly id: string;
  readonly date: Date;
  readonly registrationDeadline: Date;
  readonly timeslots: readonly MinigolfEventTimeslot[];
  readonly isStarted: boolean;
};

export type MinigolfEventTimeslot = {
  readonly id: string;
  readonly time: Time;
  readonly mapId: string;
  readonly isFallbackAllowed: boolean;
  readonly preconfigurations: readonly MinigolfEventInstancePreconfiguration[];
  readonly playerIds: readonly string[];
  readonly instances: readonly MinigolfEventInstance[];
};

export type MinigolfEventInstancePreconfiguration = {
  readonly id: string;
  readonly playerIds: readonly string[];
};

export type MinigolfEventInstance = {
  readonly id: string;
  readonly groupCode: string;
  readonly playerIds: readonly string[];
};
