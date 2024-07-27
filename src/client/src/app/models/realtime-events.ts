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
export type EventInstancesEditorChangedEvent = {
  eventId: string;
  userId: string | null | undefined;
};
export type EventPreconfigurationChangedRealtimeEvent = {
  eventId: string;
  eventTimeslotId: string;
  eventPreconfigurationId: string;
  changeType: RealtimeEventChangeType;
};
export type PlayerEventChangedRealtimeEvent = {
  eventId: string;
  changeType: RealtimeEventChangeType;
};
export type PlayerEventRegistrationChangedRealtimeEvent = {
  eventId: string;
};
export type UserSettingsChangedRealtimeEvent = {};
export type PlayerEventTimeslotRegistrationChanged = {
  eventId: string;
  eventTimeslotId: string;
  userId: string;
  isRegistered: boolean;
};
