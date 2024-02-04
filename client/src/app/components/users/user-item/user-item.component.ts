import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { User } from '../../../models/user';

@Component({
  selector: 'app-user-item',
  standalone: true,
  imports: [],
  templateUrl: './user-item.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserItemComponent {
  public user = input.required<User>();
  public showLoginType = input(false);
}
