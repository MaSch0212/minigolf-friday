import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { Player } from '../../../models/player';

@Component({
  selector: 'app-player-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './player-item.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlayerItemComponent {
  @Input({ required: true })
  public player!: Player;

  @Input()
  public showCommIcon = true;
}
