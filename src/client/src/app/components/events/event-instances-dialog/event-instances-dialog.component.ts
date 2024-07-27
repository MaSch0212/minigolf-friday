import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { produce } from 'immer';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputGroupModule } from 'primeng/inputgroup';
import { ListboxModule } from 'primeng/listbox';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { filter, firstValueFrom, pairwise } from 'rxjs';

import { hasActionFailed, isActionBusy, isActionIdle } from '../../../+state/action-state';
import {
  selectEventEditor,
  selectEventsActionState,
  setEditingEventInstancesAction,
  setEventInstancesAction,
} from '../../../+state/events';
import { userSelectors } from '../../../+state/users';
import { Event, EventInstance, EventTimeslot } from '../../../models/parsed-models';
import { AuthService } from '../../../services/auth.service';
import { Logger } from '../../../services/logger.service';
import { TranslateService } from '../../../services/translate.service';
import { notNullish } from '../../../utils/common.utils';
import { errorToastEffect, selectSignal } from '../../../utils/ngrx.utils';
import { UserItemComponent } from '../../users/user-item/user-item.component';

type EventInstances = { timeslot: EventTimeslot; instances: EventInstance[] }[];

@Component({
  selector: 'app-event-instances-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    DialogModule,
    InputGroupModule,
    ListboxModule,
    OverlayPanelModule,
    UserItemComponent,
  ],
  templateUrl: './event-instances-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventInstancesDialogComponent {
  private readonly _store = inject(Store);
  private readonly _messageService = inject(MessageService);
  private readonly _confirmService = inject(ConfirmationService);
  private readonly _authService = inject(AuthService);
  protected readonly translations = inject(TranslateService).translations;

  private readonly _removeItem: EventInstance = {
    id: '<remove>',
    groupCode: '',
    playerIds: [],
  };
  private readonly _actionState = selectSignal(selectEventsActionState('setInstances'));

  protected readonly visible = signal(false);
  protected readonly event = signal<Event | null>(null);
  protected readonly eventEditedBy = selectSignal(
    computed(() => selectEventEditor(this.event()?.id))
  );
  protected readonly instances = signal<EventInstances>([]);
  protected readonly allUsers = selectSignal(userSelectors.selectEntities);
  protected readonly unassignedUsers = computed(() =>
    Object.fromEntries(
      this.instances().map(({ timeslot, instances }) => {
        const unassignedUsers = Object.values(this.allUsers())
          .filter(notNullish)
          .filter(u => !instances.some(i => i.playerIds.includes(u.id)));

        const groups = [
          {
            label: this.translations.events_instancesDialog_registeredPlayers(),
            items: unassignedUsers.filter(u => timeslot.playerIds.includes(u.id)),
          },
          {
            label: this.translations.events_instancesDialog_unregisteredPlayers(),
            items: unassignedUsers.filter(u => !timeslot.playerIds.includes(u.id)),
          },
        ];
        return [timeslot.id, groups] as const;
      })
    )
  );
  protected readonly moveToInstanceOptions = computed(() =>
    Object.fromEntries(
      this.instances().map(
        ({ timeslot, instances }) =>
          [
            timeslot.id,
            Object.fromEntries(
              instances.map(
                instance =>
                  [
                    instance.id,
                    [
                      ...(this.instances()
                        .find(({ timeslot }) => timeslot.id === timeslot.id)
                        ?.instances.filter(i => i.id !== instance.id) ?? []),
                      this._removeItem,
                    ],
                  ] as [string, EventInstance[]]
              )
            ),
          ] as const
      )
    )
  );

  protected readonly isBusy = computed(() => isActionBusy(this._actionState()));

  constructor() {
    toObservable(this.visible)
      .pipe(
        pairwise(),
        takeUntilDestroyed(),
        filter(([prev, curr]) => prev && !curr)
      )
      .subscribe(() => {
        const event = this.event();
        if (event && this.eventEditedBy() === this._authService.user()?.id) {
          this._store.dispatch(
            setEditingEventInstancesAction({ eventId: event.id, isEditing: false })
          );
        }
        this.event.set(null);
        this.instances.set([]);
      });

    effect(
      () => {
        const eventEditedBy = this.eventEditedBy();
        if (eventEditedBy !== this._authService.user()?.id && untracked(() => this.visible())) {
          this.visible.set(false);
          const user = eventEditedBy ? this.allUsers()[eventEditedBy] : null;
          this._confirmService.confirm({
            message: this.translations.events_removeFromEditingBy(user),
            rejectVisible: false,
            acceptIcon: 'no-icon',
            acceptLabel: this.translations.shared_ok(),
          });
        }
      },
      { allowSignalWrites: true }
    );

    errorToastEffect(this.translations.events_error_changeGroups, this._actionState);

    const actions$ = inject(Actions);
    actions$
      .pipe(
        ofType(setEventInstancesAction.success),
        filter(({ props }) => props.eventId === this.event()?.id),
        takeUntilDestroyed()
      )
      .subscribe(() => this.visible.set(false));
  }

  public async open(event: Event) {
    if (event.userIdEditingInstances) {
      const user = this.allUsers()[event.userIdEditingInstances];
      const result = await new Promise<boolean>(resolve => {
        this._confirmService.confirm({
          icon: 'i-[mdi--account-lock]',
          header: this.translations.events_userIsEditingAlreadyTitle(),
          message: this.translations.events_userIsEditingAlready(user),
          acceptLabel: this.translations.events_removeUserFromEditing(user),
          acceptButtonStyleClass: 'p-button-danger',
          rejectLabel: this.translations.shared_cancel(),
          rejectButtonStyleClass: 'p-button-text',
          accept: () => resolve(true),
          reject: () => resolve(false),
        });
      });
      if (!result) return;
    }

    this._store.dispatch(setEditingEventInstancesAction({ eventId: event.id, isEditing: true }));
    const result = await firstValueFrom(
      this._store
        .select(selectEventsActionState('setInstancesEditing'))
        .pipe(filter(x => isActionIdle(x)))
    );
    if (hasActionFailed(result)) {
      this._messageService.add({
        severity: 'error',
        summary: this.translations.events_error_setEditingInstances(),
        detail: this.translations.shared_tryAgainLater(),
        life: 7500,
      });
      return;
    }

    this.event.set(event);
    this.instances.set(
      event.timeslots.map(timeslot => ({ timeslot, instances: timeslot.instances }))
    );
    this.visible.set(true);
  }

  protected addPlayer(timeslotId: string, instanceId: string, playerId: string) {
    Logger.logDebug('EventInstancesDialogComponent', 'addPlayer', {
      timeslotId,
      instanceId,
      playerId,
    });
    this.instances.update(
      produce(draft => {
        const timeslot = draft.find(({ timeslot }) => timeslot.id === timeslotId);
        const instance = timeslot?.instances.find(i => i.id === instanceId);
        instance?.playerIds.push(playerId);
      })
    );
  }

  protected movePlayer(
    timeslotId: string,
    playerId: string,
    oldInstanbceId: string,
    newInstanceId: string
  ) {
    Logger.logDebug('EventInstancesDialogComponent', 'movePlayer', {
      timeslotId,
      playerId,
      oldInstanbceId,
      newInstanceId,
    });
    this.instances.update(
      produce(draft => {
        const timeslot = draft.find(({ timeslot }) => timeslot.id === timeslotId);
        const oldInstance = timeslot?.instances.find(i => i.id === oldInstanbceId);
        const newInstance = timeslot?.instances.find(i => i.id === newInstanceId);
        oldInstance?.playerIds.splice(oldInstance.playerIds.indexOf(playerId), 1);
        newInstance?.playerIds.push(playerId);
      })
    );
  }

  protected submit() {
    const event = this.event();
    const instances = this.instances();
    Logger.logDebug('EventInstancesDialogComponent', 'submit', { event, instances });

    if (!event) return;

    this._store.dispatch(
      setEventInstancesAction({
        eventId: event.id,
        instances: instances.map(x => ({ timeslotId: x.timeslot.id, instances: x.instances })),
      })
    );
  }
}
