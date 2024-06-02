import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { MinigolfMap } from '../../../models/parsed-models';

@Component({
  selector: 'app-map-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-item.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapItemComponent {
  @Input({ required: true })
  public map!: MinigolfMap;
}
