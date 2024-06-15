export type PushNotificationDataType =
  | 'event-published'
  | 'event-started'
  | 'event-instance-updated'
  | 'event-timeslot-starting';

export type PushNotificationData<T extends PushNotificationDataType = PushNotificationDataType> = {
  'event-published': {
    type: 'event-published';
    eventId: string;
    date: string;
  };
  'event-started': {
    type: 'event-started';
    eventId: string;
  };
  'event-instance-updated': {
    type: 'event-instance-updated';
    eventId: string;
  };
  'event-timeslot-starting': {
    type: 'event-timeslot-starting';
    eventId: string;
    timeToStart: string;
    playerCount: number;
    groupCode: string;
    mapName: string;
  };
}[T];
