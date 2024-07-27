import {
  computed,
  effect,
  EventEmitter,
  inject,
  Injectable,
  OnDestroy,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  InvocationMessage,
} from '@microsoft/signalr';
import { defer, EMPTY, EmptyError, filter, firstValueFrom, map, pairwise, startWith } from 'rxjs';

import { AuthService } from './auth.service';
import { Logger } from './logger.service';
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
  PlayerEventTimeslotRegistrationChanged,
  EventInstancesEditorChangedEvent,
} from '../models/realtime-events';
import { SignalrRetryPolicy } from '../signalr-retry-policy';
import { onDocumentVisibilityChange$ } from '../utils/event.utils';
import { retryWithPolicy } from '../utils/rxjs.utils';

const USER_CHANGED = 'userChanged';
const MAP_CHANGED = 'mapChanged';
const EVENT_CHANGED = 'eventChanged';
const EVENT_TIMESLOT_CHANGED = 'eventTimeslotChanged';
const EVENT_INSTANCES_CHANGED = 'eventInstancesChanged';
const EVENT_INSTANCES_EDITOR_CHANGED = 'eventInstancesEditorChanged';
const EVENT_PRECONFIGURATION_CHANGED = 'eventPreconfigurationChanged';
const PLAYER_EVENT_CHANGED = 'playerEventChanged';
const PLAYER_EVENT_REGISTRATION_CHANGED = 'playerEventRegistrationChanged';
const USER_SETTINGS_CHANGED = 'userSettingsChanged';
const PLAYER_EVENT_TIMESLOT_REGISTRATION_CHANGED = 'playerEventTimeslotRegistrationChanged';

@Injectable({ providedIn: 'root' })
export class RealtimeEventsService implements OnDestroy {
  private readonly _authService = inject(AuthService);
  private readonly _isConnected = signal<boolean | null>(null);
  private _hubConnection?: HubConnection;

  public readonly userChanged = new EventEmitter<UserChangedRealtimeEvent>();
  public readonly mapChanged = new EventEmitter<MapChangedRealtimeEvent>();
  public readonly eventChanged = new EventEmitter<EventChangedRealtimeEvent>();
  public readonly eventTimeslotChanged = new EventEmitter<EventTimeslotChangedRealtimeEvent>();
  public readonly eventInstancesChanged = new EventEmitter<EventInstancesChangedRealtimeEvent>();
  public readonly eventInstancesEditorChanged =
    new EventEmitter<EventInstancesEditorChangedEvent>();
  public readonly eventPreconfigurationChanged =
    new EventEmitter<EventPreconfigurationChangedRealtimeEvent>();
  public readonly playerEventChanged = new EventEmitter<PlayerEventChangedRealtimeEvent>();
  public readonly playerEventRegistrationChanged =
    new EventEmitter<PlayerEventRegistrationChangedRealtimeEvent>();
  public readonly userSettingsChanged = new EventEmitter<UserSettingsChangedRealtimeEvent>();
  public readonly playerEventTimeslotRegistrationChanged =
    new EventEmitter<PlayerEventTimeslotRegistrationChanged>();
  public readonly isConnected = computed(() => !!this._isConnected());
  public readonly onReconnected$ = toObservable(this._isConnected).pipe(
    startWith(null),
    pairwise(),
    filter(([p, c]) => p === false && c === true),
    map((): void => {})
  );

  constructor() {
    effect(() => {
      const tokenInfo = this._authService.token();
      untracked(() => this.onLoginChanged(tokenInfo));
    });

    onDocumentVisibilityChange$()
      .pipe(takeUntilDestroyed())
      .subscribe(isVisible => {
        if (isVisible) {
          this.ensureConnected();
        } else {
          this.disconnect();
        }
      });
  }

  public ngOnDestroy() {
    this.disconnect();
  }

  private async ensureConnected() {
    await this._authService.ensureTokenNotExpired();
    if (!this._authService.token()) return;
    if (!this._hubConnection) {
      await this.connect();
    } else if (this._hubConnection.state !== HubConnectionState.Connected) {
      Logger.logDebug('RealtimeEventsService', 'Restarting realtime events connection');
      await this._hubConnection.stop();
      this.start();
    }
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
          Logger.logWarn(
            'RealtimeEventsService',
            `Realtime events connection lost. Retry connection in ${nextDelay}ms.`,
            error
          )
        )
      )
      .build();

    this.on(connection, USER_CHANGED, this.userChanged);
    this.on(connection, MAP_CHANGED, this.mapChanged);
    this.on(connection, EVENT_CHANGED, this.eventChanged);
    this.on(connection, EVENT_TIMESLOT_CHANGED, this.eventTimeslotChanged);
    this.on(connection, EVENT_INSTANCES_CHANGED, this.eventInstancesChanged);
    this.on(connection, EVENT_INSTANCES_EDITOR_CHANGED, this.eventInstancesEditorChanged);
    this.on(connection, EVENT_PRECONFIGURATION_CHANGED, this.eventPreconfigurationChanged);
    this.on(connection, PLAYER_EVENT_CHANGED, this.playerEventChanged);
    this.on(connection, PLAYER_EVENT_REGISTRATION_CHANGED, this.playerEventRegistrationChanged);
    this.on(connection, USER_SETTINGS_CHANGED, this.userSettingsChanged);
    this.on(
      connection,
      PLAYER_EVENT_TIMESLOT_REGISTRATION_CHANGED,
      this.playerEventTimeslotRegistrationChanged
    );

    connection.onreconnecting(() => this._isConnected.set(false));
    connection.onreconnected(() => this._isConnected.set(true));
    connection.onclose(() => this._isConnected.set(false));

    if (connection['_invokeClientMethod']) {
      const oldHandler = connection['_invokeClientMethod'];
      connection['_invokeClientMethod'] = (message: InvocationMessage) => {
        Logger.logDebug('RealtimeEventsService', 'Received realtime event:', {
          methodName: message.target,
          arguments: message.arguments,
        });
        return oldHandler.call(connection, message);
      };
    } else {
      Logger.logWarn('RealtimeEventsService', 'Cannot hook into HubConnection._invokeClientMethod');
    }

    this._hubConnection = connection;
    await this.start();
  }

  private async start() {
    Logger.logDebug('RealtimeEventsService', 'Starting realtime events connection');
    try {
      await firstValueFrom(
        defer(() =>
          !this._hubConnection || this._hubConnection.state === HubConnectionState.Connected
            ? EMPTY
            : this._hubConnection.start()
        ).pipe(
          retryWithPolicy(
            new SignalrRetryPolicy((error, nextDelay) =>
              Logger.logWarn(
                'RealtimeEventsService',
                `Realtime events connection unsuccessful. Retry connection in ${nextDelay}ms.`,
                error
              )
            )
          )
        )
      );
      Logger.logDebug(
        'RealtimeEventsService',
        'Realtime events connection started with state',
        this._hubConnection?.state
      );
      if (this._hubConnection?.state === HubConnectionState.Connected) {
        this._isConnected.set(true);
      }
    } catch (ex) {
      if (!(ex instanceof EmptyError)) {
        Logger.logError('RealtimeEventsService', 'Failed to start realtime events connection', ex);
        throw ex;
      } else {
        Logger.logDebug('RealtimeEventsService', 'Realtime events connection already started');
      }
    }
  }

  private on<T>(connection: HubConnection, name: string, eventEmitter: EventEmitter<T>) {
    connection.on(name, (event: T) => eventEmitter.emit(event));
  }

  private async disconnect() {
    if (this._hubConnection) {
      Logger.logDebug('RealtimeEventsService', 'Stopping realtime events connection');
      const connection = this._hubConnection;
      this._hubConnection = undefined;
      await connection.stop();
    }
  }
}
