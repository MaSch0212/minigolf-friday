import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DataViewModule } from 'primeng/dataview';
import { InputTextModule } from 'primeng/inputtext';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { loadPlayersAction, playerSelectors, selectPlayersLoadState } from '../../+state/players';
import { interpolate } from '../../directives/interpolate.pipe';
import { Player } from '../../models/player';
import { TranslateService } from '../../services/translate.service';
import { notNullish } from '../../utils/common.utils';

function playerMatchesFilter(
  player: Player | undefined,
  lowerCaseFilter: string
): player is Player {
  return !!(
    player &&
    (player.name.toLowerCase().includes(lowerCaseFilter) ||
      player.alias?.toLowerCase().includes(lowerCaseFilter))
  );
}

@Component({
  selector: 'app-players',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    DataViewModule,
    FormsModule,
    InputTextModule,
    MessagesModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './players.component.html',
  styleUrl: './players.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayersComponent implements OnInit {
  private readonly _store = inject(Store);
  private readonly _allPlayers = this._store.selectSignal(playerSelectors.selectAll);
  private readonly _confirmationService = inject(ConfirmationService);
  private readonly _messageService = inject(MessageService);

  protected readonly translations = inject(TranslateService).translations;
  protected readonly filter = signal('');
  protected readonly players = computed(() =>
    this.filterPlayers(this._allPlayers(), this.filter())
  );
  protected readonly loadState = this._store.selectSignal(selectPlayersLoadState);

  public ngOnInit() {
    this._store.dispatch(loadPlayersAction({ reload: false }));
  }

  protected trackByPlayerId = (_: number, player: Player) => player.id;

  protected deletePlayer(player: Player) {
    this._confirmationService.confirm({
      header: this.translations.players_deleteDialog_title(),
      message: interpolate(this.translations.players_deleteDialog_text(), player),
      acceptLabel: this.translations.players_deleteDialog_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left mdi mdi-delete-outline',
      rejectLabel: this.translations.players_deleteDialog_cancel(),
      accept: () => {
        this._messageService.add({
          severity: 'success',
          summary: this.translations.players_playerDeleted_summary(),
          detail: interpolate(this.translations.players_playerDeleted_detail(), player),
          life: 2000,
        });
      },
    });
  }

  private filterPlayers(players: (Player | undefined)[], filter: string): Player[] {
    if (!filter) {
      return players.filter(notNullish);
    }
    const lowerCaseFilter = filter.toLowerCase();
    return players.filter((p): p is Player => playerMatchesFilter(p, lowerCaseFilter));
  }
}
