import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';

import { InviteAdminDialogComponent } from './invite-admin-dialog/invite-admin-dialog.component';
import { TranslateService } from '../../services/translate.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [ButtonModule, CommonModule, InviteAdminDialogComponent],
  templateUrl: './users.component.html',
  styles: ``,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent {
  private readonly _translateService = inject(TranslateService);

  protected translations = this._translateService.translations;
}
