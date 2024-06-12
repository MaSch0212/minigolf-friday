import { HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Actions, ofType } from '@ngrx/effects';
import { ReducerTypes, Store, createAction, on } from '@ngrx/store';
import { Action, Selector } from '@ngrx/store/src/models';
import { produce } from 'immer';
import { catchError, filter, from, map, of, pipe, startWith, withLatestFrom } from 'rxjs';

export type ActionState = {
  state: 'none' | 'starting' | 'running' | 'success' | 'error';
  error?: unknown;
};

export const initialActionState: ActionState = { state: 'none' };
export const startingActionState: ActionState = { state: 'starting' };
export const runningActionState: ActionState = { state: 'running' };
export const successActionState: ActionState = { state: 'success' };
export const errorActionState = (error: unknown): ActionState => ({ state: 'error', error });

export function isActionBusy(actionState: ActionState): boolean {
  return actionState.state === 'starting' || actionState.state === 'running';
}

export function hasActionFailed(actionState: ActionState, excludedStatusCodes?: number[]): boolean {
  if (excludedStatusCodes && actionState.error instanceof HttpErrorResponse) {
    return actionState.state === 'error' && !excludedStatusCodes.includes(actionState.error.status);
  }
  return actionState.state === 'error';
}

export function hasActionSucceeded(actionState: ActionState): boolean {
  return actionState.state === 'success';
}

export function isActionIdle(actionState: ActionState): boolean {
  return (
    actionState.state === 'none' || actionState.state === 'success' || actionState.state === 'error'
  );
}

type _Creator<TProps> = (props: TProps) => { props: TProps };
export function creator<TProps>(): _Creator<TProps> {
  return (props: TProps) => ({ props });
}
type _CreatorWithResponse<TProps, TResponse> = (
  props: TProps,
  response: TResponse
) => { props: TProps; response: TResponse };
export function creatorWithResponse<TProps, TResponse>(): _CreatorWithResponse<TProps, TResponse> {
  return (props: TProps, response: TResponse) => ({ props, response });
}
type _ActionCreator<TName extends string, TProps> = ((
  props: TProps
) => { props: TProps } & Action<TName>) &
  Action<TName>;
type _ActionCreatorWithResponse<TName extends string, TProps, TResponse> = ((
  props: TProps,
  response: TResponse
) => { props: TProps; response: TResponse } & Action<TName>) &
  Action<TName>;
export type HttpActionCreator<
  TScope extends string,
  TName extends string,
  TProps,
  TSuccess,
> = _ActionCreator<`[${TScope}] ${TName}`, TProps> & {
  starting: _ActionCreator<`[${TScope}] [Starting] ${TName} `, TProps>;
  success: _ActionCreatorWithResponse<`[${TScope}] [Success] ${TName}`, TProps, TSuccess>;
  error: _ActionCreatorWithResponse<`[${TScope}] [Error] ${TName}`, TProps, unknown>;
};

type x = HttpActionCreator<'a', 'b', { a: string }, { b: number }>;

export function createHttpAction<TProps extends object, TSuccess = unknown>() {
  return <TScope extends string, TName extends string>(scope: TScope, name: TName) => {
    const action = createAction(`[${scope}] ${name}`, creator<TProps>());
    const starting = createAction(`[${scope}] [Starting] ${name}`, creator<TProps>());
    const success = createAction(
      `[${scope}] [Success] ${name}`,
      creatorWithResponse<TProps, TSuccess>()
    );
    const error = createAction(
      `[${scope}] [Error] ${name}`,
      creatorWithResponse<TProps, unknown>()
    );
    return Object.assign(action, { starting, success, error }) as HttpActionCreator<
      TScope,
      TName,
      TProps,
      TSuccess
    >;
  };
}

export function handleHttpAction<
  TState extends { actionStates: Record<string, ActionState> },
  TActionStateName extends TInferredState extends { actionStates: Record<string, ActionState> }
    ? keyof TInferredState['actionStates']
    : never,
  TAction extends HttpActionCreator<string, string, any, any>,
  TProps = Parameters<TAction>[0],
  TInferredState = TState,
>(
  actionStateName: TActionStateName,
  action: TAction,
  startCondition: (state: TInferredState, props: TProps) => boolean = () => true
): ReducerTypes<
  TInferredState,
  [TAction, TAction['starting'], TAction['success'], TAction['error']]
> {
  return on(
    action,
    action.starting,
    action.success,
    action.error,
    produce((draft, props) => {
      const actionStates = (draft as TState).actionStates;
      if (props.type === action.type) {
        if (
          !isActionBusy(actionStates[actionStateName as string]) &&
          startCondition(draft as TInferredState, props.props)
        ) {
          actionStates[actionStateName as string] = startingActionState;
        }
      } else if (props.type === action.starting.type) {
        actionStates[actionStateName as string] = runningActionState;
      } else if (props.type === action.success.type) {
        actionStates[actionStateName as string] = successActionState;
      } else if (props.type === action.error.type) {
        actionStates[actionStateName as string] = errorActionState((props as any).response);
      }
    })
  );
}

export function onHttpAction<T extends HttpActionCreator<string, string, any, any>>(
  action: T,
  actionStateSelector: Selector<object, ActionState>
) {
  return inject(Actions).pipe(
    ofType(action),
    withLatestFrom(inject(Store).select(actionStateSelector)),
    filter(([, actionState]) => actionState.state === 'starting'),
    map(([props]) => props)
  );
}

export function mapToHttpAction<
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  TAction extends HttpActionCreator<string, string, any, any>,
  TProps = Parameters<TAction>[0],
  TSuccess = Parameters<TAction['success']>[1],
>(action: TAction, props: TProps) {
  return pipe(
    map<TSuccess, Action>(response => action.success(props, response)),
    catchError(error => of(action.error(props, error))),
    startWith(action.starting(props))
  );
}

export function toHttpAction<
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  TAction extends HttpActionCreator<string, string, any, any>,
  TProps = Parameters<TAction>[0],
>(
  value: Promise<ReturnType<TAction['success'] | TAction['error']>>,
  action: TAction,
  props: TProps
) {
  return from(value).pipe(
    catchError(error => of(action.error(props, error))),
    startWith(action.starting(props))
  );
}
