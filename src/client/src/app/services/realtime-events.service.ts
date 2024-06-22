import {
  effect,
  EventEmitter,
  inject,
  Injectable,
  OnDestroy,
  signal,
  untracked,
} from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { defer, EMPTY, EmptyError, firstValueFrom } from 'rxjs';

import { AuthService } from './auth.service';
import { AuthTokenInfo } from './storage';
import {
  UserChangedRealtimeEvent,
  MapChangedRealtimeEvent,
  EventChangedRealtimeEvent,
  EventTimeslotChangedRealtimeEvent,
  EventInstancesChangedRealtimeEvent,
  EventPreconfigurationChangedRealtimeEvent,
  PlayerEventChangedRealtimeEvent,
  PlayerEventRegistrationChangedRealtimeEvent,
  UserSettingsChangedRealtimeEvent,
} from '../models/realtime-events';
import { SignalrRetryPolicy } from '../signalr-retry-policy';
import { retryWithPolicy } from '../utils/rxjs.utils';

@Injectable({ providedIn: 'root' })
export class RealtimeEventsService implements OnDestroy {
  private readonly _authService = inject(AuthService);
  private readonly _isConnected = signal<boolean>(false);
  private _hubConnection?: HubConnection;

  public readonly userChanged = new EventEmitter<UserChangedRealtimeEvent>();
  public readonly mapChanged = new EventEmitter<MapChangedRealtimeEvent>();
  public readonly eventChanged = new EventEmitter<EventChangedRealtimeEvent>();
  public readonly eventTimeslotChanged = new EventEmitter<EventTimeslotChangedRealtimeEvent>();
  public readonly eventInstancesChanged = new EventEmitter<EventInstancesChangedRealtimeEvent>();
  public readonly eventPreconfigurationChanged =
    new EventEmitter<EventPreconfigurationChangedRealtimeEvent>();
  public readonly playerEventChanged = new EventEmitter<PlayerEventChangedRealtimeEvent>();
  public readonly playerEventRegistrationChanged =
    new EventEmitter<PlayerEventRegistrationChangedRealtimeEvent>();
  public readonly userSettingsChanged = new EventEmitter<UserSettingsChangedRealtimeEvent>();
  public readonly isConnected = this._isConnected.asReadonly();

  constructor() {
    effect(() => {
      untracked(() => this.onLoginChanged(this._authService.token()));
    });
  }

  public ngOnDestroy() {
    this.disconnect();
  }

  private onLoginChanged(token: AuthTokenInfo | null | undefined) {
    if (token) {
      this.connect();
    } else {
      this.disconnect();
    }
  }

  private async connect() {
    if (this._hubConnection) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl('/hubs/realtime-events', {
        accessTokenFactory: () => this._authService.token()?.token ?? '',
        withCredentials: false,
      })
      .withAutomaticReconnect(
        new SignalrRetryPolicy((error, nextDelay) =>
          console.warn(
            `Realtime events connection lost. Retry connection in ${nextDelay}ms.`,
            error,
            nextDelay
          )
        )
      )
      .build();

    connection.on('userChanged', (event: UserChangedRealtimeEvent) => this.userChanged.emit(event));
    connection.on('mapChanged', (event: MapChangedRealtimeEvent) => this.mapChanged.emit(event));
    connection.on('eventChanged', (event: EventChangedRealtimeEvent) =>
      this.eventChanged.emit(event)
    );
    connection.on('eventTimeslotChanged', (event: EventTimeslotChangedRealtimeEvent) =>
      this.eventTimeslotChanged.emit(event)
    );
    connection.on('eventInstancesChanged', (event: EventInstancesChangedRealtimeEvent) =>
      this.eventInstancesChanged.emit(event)
    );
    connection.on(
      'eventPreconfigurationChanged',
      (event: EventPreconfigurationChangedRealtimeEvent) =>
        this.eventPreconfigurationChanged.emit(event)
    );
    connection.on('playerEventChanged', (event: PlayerEventChangedRealtimeEvent) =>
      this.playerEventChanged.emit(event)
    );
    connection.on(
      'playerEventRegistrationChanged',
      (event: PlayerEventRegistrationChangedRealtimeEvent) =>
        this.playerEventRegistrationChanged.emit(event)
    );
    connection.on('userSettingsChanged', (event: UserSettingsChangedRealtimeEvent) =>
      this.userSettingsChanged.emit(event)
    );

    connection.onreconnecting(() => this._isConnected.set(false));
    connection.onreconnected(() => this._isConnected.set(true));
    connection.onclose(() => this._isConnected.set(false));

    this._hubConnection = connection;
    try {
      await firstValueFrom(
        defer(() => this._hubConnection?.start() ?? EMPTY).pipe(
          retryWithPolicy(
            new SignalrRetryPolicy((error, nextDelay) =>
              console.warn(
                `Realtime events connection unsuccessful. Retry connection in ${nextDelay}ms.`,
                error
              )
            )
          )
        )
      );
      this._isConnected.set(true);
    } catch (ex) {
      if (!(ex instanceof EmptyError)) {
        throw ex;
      }
    }
  }

  private async disconnect() {
    if (this._hubConnection) {
      const connection = this._hubConnection;
      this._hubConnection = undefined;
      await connection.stop();
    }
  }
}
