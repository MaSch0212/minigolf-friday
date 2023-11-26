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
import { ButtonModule } from 'primeng/button';
import { DataViewModule } from 'primeng/dataview';
import { InputTextModule } from 'primeng/inputtext';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import {
  PlayersFeatureSlice,
  loadPlayersAction,
  playerSelectors,
  selectPlayersLoadState,
} from '../../+state/players';
import { Player } from '../../models/player';
import { TranslateService } from '../../services/translate.service';
import { notNullish } from '../../utils/common.utils';
import { injectStore } from '../../utils/store.utils';

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
  private readonly _store = injectStore<PlayersFeatureSlice>();
  private readonly _allPlayers = this._store.selectSignal(playerSelectors.selectAll);

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

  private filterPlayers(players: (Player | undefined)[], filter: string): Player[] {
    if (!filter) {
      return players.filter(notNullish);
    }
    const lowerCaseFilter = filter.toLowerCase();
    return players.filter((p): p is Player => playerMatchesFilter(p, lowerCaseFilter));
  }
}
