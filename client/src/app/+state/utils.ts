import { FunctionalEffect } from '@ngrx/effects';
import { ActionCreator, ReducerTypes } from '@ngrx/store';

export type Effects = Record<string, FunctionalEffect>;

export type Reducers<TState> = ReducerTypes<TState, readonly ActionCreator[]>[];
