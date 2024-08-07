import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Actions, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { MapDialogComponent } from './map-dialog/map-dialog.component';
import { MapItemComponent } from './map-item/map-item.component';
import { hasActionFailed, isActionBusy } from '../../+state/action-state';
import { mapSelectors, removeMapAction, selectMapsActionState } from '../../+state/maps';
import { keepMapsLoaded } from '../../+state/maps/maps.utils';
import { MinigolfMap } from '../../models/parsed-models';
import { TranslateService } from '../../services/translate.service';
import { notNullish } from '../../utils/common.utils';
import { selectSignal } from '../../utils/ngrx.utils';

function mapMatchesFilter(
  map: MinigolfMap | undefined,
  lowerCaseFilter: string
): map is MinigolfMap {
  return !!(map && map.name.toLowerCase().includes(lowerCaseFilter));
}

@Component({
  selector: 'app-maps',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    FormsModule,
    InputTextModule,
    MapDialogComponent,
    MapItemComponent,
    MessagesModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './maps.component.html',
  styleUrl: './maps.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapsComponent {
  private readonly _store = inject(Store);
  private readonly _allMaps = selectSignal(mapSelectors.selectAll);
  private readonly _confirmationService = inject(ConfirmationService);
  private readonly _messageService = inject(MessageService);

  private readonly _actionState = selectSignal(selectMapsActionState('load'));

  protected readonly translations = inject(TranslateService).translations;
  protected readonly filter = signal('');
  protected readonly maps = computed(() => this.filterMaps(this._allMaps(), this.filter()));
  protected readonly isLoading = computed(() => isActionBusy(this._actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this._actionState()));

  constructor() {
    keepMapsLoaded();

    const action$ = inject(Actions);
    action$
      .pipe(ofType(removeMapAction.error), takeUntilDestroyed())
      .subscribe(({ props: { mapId } }) =>
        this.onMapDeletionFailed(
          this.maps().find(({ id }) => id === mapId) ?? { id: mapId, name: '' }
        )
      );
  }

  protected deleteMap(map: MinigolfMap) {
    this._confirmationService.confirm({
      header: this.translations.maps_deleteDialog_title(),
      message: this.translations.maps_deleteDialog_text(map),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () => this._store.dispatch(removeMapAction({ mapId: map.id })),
    });
  }

  private filterMaps(maps: (MinigolfMap | undefined)[], filter: string): MinigolfMap[] {
    if (!filter) {
      return maps.filter(notNullish);
    }
    const lowerCaseFilter = filter.toLowerCase();
    return maps.filter((map): map is MinigolfMap => mapMatchesFilter(map, lowerCaseFilter));
  }

  private onMapDeletionFailed(map: MinigolfMap) {
    this._messageService.add({
      severity: 'error',
      summary: this.translations.maps_error_delete(),
      detail: this.translations.shared_tryAgainLater(map),
      life: 2000,
    });
  }
}
