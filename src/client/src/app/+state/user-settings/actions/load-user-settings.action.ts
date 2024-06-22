import { inject } from '@angular/core';
import { on } from '@ngrx/store';
import { produce } from 'immer';
import { switchMap } from 'rxjs';

import { UserSettingsService } from '../../../api/services';
import { UserSettings } from '../../../models/parsed-models';
import { assertBody } from '../../../utils/http.utils';
import { createHttpAction, handleHttpAction, onHttpAction, toHttpAction } from '../../action-state';
import { createFunctionalEffect } from '../../functional-effect';
import { Effects, Reducers } from '../../utils';
import { USER_SETTINGS_ACTION_SCOPE } from '../consts';
import { selectUserSettingsActionState } from '../user-settings.selectors';
import { UserSettingsFeatureState } from '../user-settings.state';

export const loadUserSettingsAction = createHttpAction<
  { reload?: boolean; silent?: boolean },
  UserSettings
>()(USER_SETTINGS_ACTION_SCOPE, 'Load');

export const loadUserSettingsReducers: Reducers<UserSettingsFeatureState> = [
  on(
    loadUserSettingsAction.success,
    produce((draft, { response }) => {
      draft.settings = response;
    })
  ),
  handleHttpAction('load', loadUserSettingsAction, {
    condition: (s, p) => !p.silent,
    startCondition: (s, p) => !s.settings || !!p.reload,
  }),
];

export const loadUserSettingsEffects: Effects = {
  loadUserSettings$: createFunctionalEffect.dispatching((api = inject(UserSettingsService)) =>
    onHttpAction(
      loadUserSettingsAction,
      selectUserSettingsActionState('load'),
      p => !!p.props.silent
    ).pipe(
      switchMap(({ props }) =>
        toHttpAction(loadUserSettings(api, props), loadUserSettingsAction, props)
      )
    )
  ),
};

async function loadUserSettings(
  api: UserSettingsService,
  props: ReturnType<typeof loadUserSettingsAction>['props']
) {
  const response = await api.getUserSettings();
  return response.ok
    ? loadUserSettingsAction.success(props, assertBody(response).settings)
    : loadUserSettingsAction.error(props, response);
}
