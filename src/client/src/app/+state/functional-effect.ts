import { inject } from '@angular/core';
import { Actions, EffectConfig, createEffect, ofType } from '@ngrx/effects';
import { ActionCreator, Action } from '@ngrx/store';
import { Observable } from 'rxjs';

export type FunctionalEffectConfig = Omit<EffectConfig, 'functional' | 'dispatch'>;
const defaultFunctionalEffectConfig = {
  functional: true,
  dispatch: false,
} satisfies EffectConfig;
const defaultFunctionalEffectConfigWithDispatch = {
  functional: true,
  dispatch: true,
} satisfies EffectConfig;

function _createFunctionalEffect<Source extends () => Observable<unknown>>(
  source: Source,
  config?: FunctionalEffectConfig
) {
  const actualConfig = config
    ? { ...defaultFunctionalEffectConfig, ...config }
    : defaultFunctionalEffectConfig;
  return createEffect(source, actualConfig);
}

type EffectResult<OT> = Observable<OT> | ((...args: any[]) => Observable<OT>);
type ConditionallyDisallowActionCreator<DT, Result> = DT extends false
  ? unknown
  : Result extends EffectResult<infer OT>
    ? OT extends ActionCreator
      ? 'ActionCreator cannot be dispatched. Did you forget to call the action creator function?'
      : unknown
    : unknown;
function _createDispatchingFunctionalEffect<Source extends () => Observable<Action>>(
  source: Source & ConditionallyDisallowActionCreator<true, ReturnType<Source>>,
  config?: FunctionalEffectConfig
) {
  const actualConfig = config
    ? { ...defaultFunctionalEffectConfigWithDispatch, ...config }
    : defaultFunctionalEffectConfigWithDispatch;
  return createEffect(source, actualConfig);
}

export type CreateFunctionalEffectFunction = typeof _createFunctionalEffect & {
  dispatching: typeof _createDispatchingFunctionalEffect;
};

export const createFunctionalEffect: CreateFunctionalEffectFunction = (() => {
  (_createFunctionalEffect as any).dispatching = _createDispatchingFunctionalEffect;
  return _createFunctionalEffect as CreateFunctionalEffectFunction;
})();

export function actionOfType<T extends ActionCreator>(action: T) {
  return inject(Actions).pipe(ofType(action));
}
