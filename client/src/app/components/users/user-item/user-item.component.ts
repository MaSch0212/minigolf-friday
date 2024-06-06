import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { User } from '../../../models/parsed-models';

@Component({
  selector: 'app-user-item',
  standalone: true,
  imports: [],
  templateUrl: './user-item.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserItemComponent {
  public user = input.required<User>();
}
