import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
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
import {
  deleteMapAction,
  deleteMapFailureAction,
  loadMapsAction,
  mapSelectors,
  selectMapsLoadState,
} from '../../+state/maps';
import { interpolate } from '../../directives/interpolate.pipe';
import { MinigolfMap } from '../../models/minigolf-map';
import { TranslateService } from '../../services/translate.service';
import { notNullish } from '../../utils/common.utils';
import { autoDestroy } from '../../utils/rxjs.utils';

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
    MessagesModule,
    MapDialogComponent,
    MapItemComponent,
    ProgressSpinnerModule,
  ],
  templateUrl: './maps.component.html',
  styleUrl: './maps.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapsComponent implements OnInit {
  private readonly _store = inject(Store);
  private readonly _allMaps = this._store.selectSignal(mapSelectors.selectAll);
  private readonly _confirmationService = inject(ConfirmationService);
  private readonly _messageService = inject(MessageService);

  protected readonly translations = inject(TranslateService).translations;
  protected readonly filter = signal('');
  protected readonly maps = computed(() => this.filterMaps(this._allMaps(), this.filter()));
  protected readonly loadState = this._store.selectSignal(selectMapsLoadState);

  constructor() {
    const action$ = inject(Actions);
    autoDestroy(
      action$
        .pipe(ofType(deleteMapFailureAction))
        .subscribe(({ map }) => this.onMapDeletionFailed(map))
    );
    effect(() => console.log(this.maps()));
  }

  public ngOnInit(): void {
    this._store.dispatch(loadMapsAction({ reload: false }));
  }

  protected trackByMapId = (_: number, map: MinigolfMap) => map.id;

  protected deleteMap(map: MinigolfMap) {
    this._confirmationService.confirm({
      header: this.translations.maps_deleteDialog_title(),
      message: interpolate(this.translations.maps_deleteDialog_text(), map),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete-outline]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () => this._store.dispatch(deleteMapAction({ map })),
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
      detail: interpolate(this.translations.shared_tryAgainLater(), map),
      life: 2000,
    });
  }
}
