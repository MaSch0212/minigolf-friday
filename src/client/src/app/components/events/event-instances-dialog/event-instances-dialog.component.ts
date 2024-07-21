import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { produce } from 'immer';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputGroupModule } from 'primeng/inputgroup';
import { ListboxModule } from 'primeng/listbox';
import { OverlayPanelModule } from 'primeng/overlaypanel';

import { userSelectors } from '../../../+state/users';
import { Event, EventInstance, EventTimeslot } from '../../../models/parsed-models';
import { Logger } from '../../../services/logger.service';
import { TranslateService } from '../../../services/translate.service';
import { notNullish } from '../../../utils/common.utils';
import { selectSignal } from '../../../utils/ngrx.utils';
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
  protected readonly translations = inject(TranslateService).translations;

  private readonly _removeItem: EventInstance = {
    id: '<remove>',
    groupCode: '',
    playerIds: [],
  };

  protected readonly visible = signal(false);
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

  protected readonly isBusy = signal(false);

  public open(event: Event) {
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
    Logger.logDebug('EventInstancesDialogComponent', 'submit', { instances: this.instances() });
    // TODO: Implement Server-side logic and dispatch action
  }
}
