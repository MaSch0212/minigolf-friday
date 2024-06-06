import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessagesModule } from 'primeng/messages';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { UserDialogComponent } from './user-dialog/user-dialog.component';
import { UserItemComponent } from './user-item/user-item.component';
import { isActionBusy, hasActionFailed } from '../../+state/action-state';
import {
  loadUsersAction,
  removeUserAction,
  selectUsersActionState,
  userSelectors,
} from '../../+state/users';
import { interpolate } from '../../directives/interpolate.pipe';
import { User } from '../../models/parsed-models';
import { TranslateService } from '../../services/translate.service';
import { notNullish } from '../../utils/common.utils';
import { selectSignal } from '../../utils/ngrx.utils';

function userMatchesFilter(map: User | undefined, lowerCaseFilter: string): map is User {
  return !!(map && map.alias.toLowerCase().includes(lowerCaseFilter));
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    FormsModule,
    InputTextModule,
    MessagesModule,
    UserDialogComponent,
    UserItemComponent,
    ProgressSpinnerModule,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent {
  private readonly _store = inject(Store);
  private readonly _allUsers = selectSignal(userSelectors.selectAll);
  private readonly _confirmationService = inject(ConfirmationService);
  private readonly _messageService = inject(MessageService);

  private readonly _actionState = selectSignal(selectUsersActionState('load'));

  protected readonly translations = inject(TranslateService).translations;
  protected readonly filter = signal('');
  protected readonly users = computed(() => this.filterUsers(this._allUsers(), this.filter()));
  protected readonly isLoading = computed(() => isActionBusy(this._actionState()));
  protected readonly hasFailed = computed(() => hasActionFailed(this._actionState()));

  constructor() {
    this._store.dispatch(loadUsersAction({ reload: false }));
  }

  protected deleteUser(user: User) {
    this._confirmationService.confirm({
      header: this.translations.users_deleteDialog_title(),
      message: interpolate(this.translations.users_deleteDialog_text(), user),
      acceptLabel: this.translations.shared_delete(),
      acceptButtonStyleClass: 'p-button-danger',
      acceptIcon: 'p-button-icon-left i-[mdi--delete]',
      rejectLabel: this.translations.shared_cancel(),
      rejectButtonStyleClass: 'p-button-text',
      accept: () => this._store.dispatch(removeUserAction({ userId: user.id })),
    });
  }

  private filterUsers(maps: (User | undefined)[], filter: string): User[] {
    if (!filter) {
      return maps.filter(notNullish);
    }
    const lowerCaseFilter = filter.toLowerCase();
    return maps.filter((map): map is User => userMatchesFilter(map, lowerCaseFilter));
  }
}
