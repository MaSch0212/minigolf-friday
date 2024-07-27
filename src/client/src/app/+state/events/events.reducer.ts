import { createReducer } from '@ngrx/store';

import { addEventPreconfigReducers } from './actions/add-event-preconfig.action';
import { addEventTimeslotReducers } from './actions/add-event-timeslot.action';
import { addEventReducers } from './actions/add-event.action';
import { addPlayerToEventPreconfigurationReducers } from './actions/add-player-to-preconfig.action';
import { buildEventInstancesReducers } from './actions/build-event-instances.action';
import { eventTimeslotRegistrationChangedReducers } from './actions/event-timeslot-registration-changed.action';
import { loadEventReducers } from './actions/load-event.action';
import { loadEventsReducers } from './actions/load-events.action';
import { removeEventPreconfigReducers } from './actions/remove-event-preconfig.action';
import { removeEventTimeslotReducers } from './actions/remove-event-timeslot.action';
import { removeEventReducers } from './actions/remove-event.action';
import { removePlayerFromPreconfigReducers } from './actions/remove-player-from-preconfig.action';
import { resetEventsActionStateReducers } from './actions/reset-events-action-state.action';
import { setEditingEventInstancesReducers } from './actions/set-editing-event-instances.action';
import { setEventInstancesReducers } from './actions/set-event-instances.action';
import { startEventReducers } from './actions/start-event.action';
import { updateEventTimeslotReducers } from './actions/update-event-timeslot.action';
import { updateEventReducers } from './actions/update-event.action';
import { EventsFeatureState, initialEventsFeatureState } from './events.state';

export const eventsReducer = createReducer<EventsFeatureState>(
  initialEventsFeatureState,

  ...addEventPreconfigReducers,
  ...addEventTimeslotReducers,
  ...addEventReducers,
  ...addPlayerToEventPreconfigurationReducers,
  ...buildEventInstancesReducers,
  ...eventTimeslotRegistrationChangedReducers,
  ...loadEventReducers,
  ...loadEventsReducers,
  ...removeEventPreconfigReducers,
  ...removeEventTimeslotReducers,
  ...removeEventReducers,
  ...removePlayerFromPreconfigReducers,
  ...resetEventsActionStateReducers,
  ...setEditingEventInstancesReducers,
  ...setEventInstancesReducers,
  ...startEventReducers,
  ...updateEventTimeslotReducers,
  ...updateEventReducers
);
