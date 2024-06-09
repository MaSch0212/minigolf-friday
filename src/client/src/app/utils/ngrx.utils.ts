import { Signal, computed, effect, inject, isSignal } from '@angular/core';
import { Selector, Store } from '@ngrx/store';
import { MessageService } from 'primeng/api';

import { OptionalInjector, injectEx } from './angular.utils';
import { chainSignals } from './signal.utils';
import { ActionState, hasActionFailed } from '../+state/action-state';
import { TranslateService } from '../services/translate.service';

export function selectSignal<T>(
  selector: Signal<Selector<object, T>> | Selector<object, T>,
  options?: OptionalInjector
): Signal<T> {
  const store = injectEx(Store, options);
  if (isSignal(selector)) {
    return chainSignals(
      selector,
      s => computed(() => store.selectSignal(s())),
      (_, s) => computed(() => s()())
    );
  } else {
    return store.selectSignal(selector);
  }
}

export function errorToastEffect(
  message: Signal<string>,
  actionState: Signal<ActionState> | Selector<object, ActionState>
) {
  const ast = isSignal(actionState) ? actionState : selectSignal(actionState);
  const messageService = inject(MessageService);
  const translations = inject(TranslateService).translations;
  effect(() => {
    if (hasActionFailed(ast())) {
      messageService.add({
        severity: 'error',
        summary: message(),
        detail: translations.shared_tryAgainLater(),
        life: 7500,
      });
    }
  });
}
