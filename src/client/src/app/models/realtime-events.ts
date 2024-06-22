export type RealtimeEventChangeType = 'created' | 'updated' | 'deleted';

export type UserChangedRealtimeEvent = {
  userId: string;
  changeType: RealtimeEventChangeType;
};
export type MapChangedRealtimeEvent = {
  mapId: string;
  changeType: RealtimeEventChangeType;
};
export type EventChangedRealtimeEvent = {
  eventId: string;
  changeType: RealtimeEventChangeType;
};
export type EventTimeslotChangedRealtimeEvent = {
  eventId: string;
  eventTimeslotId: string;
  changeType: RealtimeEventChangeType;
};
export type EventInstancesChangedRealtimeEvent = {
  eventId: string;
};
export type EventPreconfigurationChangedRealtimeEvent = {
  eventId: string;
  eventPreconfigurationId: string;
  changeType: RealtimeEventChangeType;
};
export type PlayerEventChangedRealtimeEvent = {
  eventId: string;
  changeType: RealtimeEventChangeType;
};
export type PlayerEventRegistrationChangedRealtimeEvent = {
  eventId: string;
  changeType: RealtimeEventChangeType;
};
export type UserSettingsChangedRealtimeEvent = {};
