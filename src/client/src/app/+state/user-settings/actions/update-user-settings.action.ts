import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { UserSettingsService } from '../../../api/services';
import { UserSettings } from '../../../models/parsed-models';
import { isEmptyObject, removeUndefinedProperties } from '../../../utils/common.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USER_SETTINGS_ACTION_SCOPE } from '../consts';
import { selectUserSettingsActionState } from '../users.selectors';
import { UserSettingsFeatureState } from '../users.state';

export const updateUserSettingsAction = createHttpAction<Partial<UserSettings>>()(
  USER_SETTINGS_ACTION_SCOPE,
  'Update'
);

export const updateUserSettingsReducers: Reducers<UserSettingsFeatureState> = [
  on(
    updateUserSettingsAction.success,
    produce((draft, { props }) => {
      if (draft.settings) {
        draft.settings = { ...draft.settings, ...removeUndefinedProperties(props) };
      }
    })
  ),
  handleHttpAction(
    'update',
    updateUserSettingsAction,
    (s, p) => !!s.settings && isEmptyObject(p, { ignoreUndefinedProperties: true })
  ),
];

export const updateUserSettingsEffects: Effects = {
  updateUserSettings$: createFunctionalEffect.dispatching((api = inject(UserSettingsService)) =>
    onHttpAction(updateUserSettingsAction, selectUserSettingsActionState('update')).pipe(
      switchMap(({ props }) =>
        toHttpAction(updateUserSettings(api, props), updateUserSettingsAction, props)
      )
    )
  ),
};

async function updateUserSettings(
  api: UserSettingsService,
  props: ReturnType<typeof updateUserSettingsAction>['props']
) {
  const response = await api.updateUserSettings({ body: props });
  return response.ok
    ? updateUserSettingsAction.success(props, undefined)
    : updateUserSettingsAction.error(props, response);
}
