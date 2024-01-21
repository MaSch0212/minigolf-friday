import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';

import { IdPipe } from '../../../directives/id.pipe';
import { InterpolatePipe } from '../../../directives/interpolate.pipe';
import { TranslateService } from '../../../services/translate.service';
import { UserInvitesService } from '../../../services/user-invites.service';

@Component({
  selector: 'app-invite-admin-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    DialogModule,
    FormsModule,
    IdPipe,
    InputTextModule,
    InterpolatePipe,
    ProgressSpinnerModule,
    TooltipModule,
  ],
  templateUrl: './invite-admin-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InviteAdminDialogComponent {
  private readonly _userInvitesService = inject(UserInvitesService);
  private readonly _translateService = inject(TranslateService);

  protected translations = this._translateService.translations;
  protected language = this._translateService.language;
  protected visible = signal(false);
  protected inviteUrl = signal<string | null>(null);
  protected inviteExpiresAt = signal<Date | null>(null);

  constructor() {
    effect(
      () => {
        if (this.visible()) {
          this.createInvite();
        } else {
          this.inviteUrl.set(null);
          this.inviteExpiresAt.set(null);
        }
      },
      { allowSignalWrites: true }
    );
  }

  public async open() {
    this.visible.set(true);
  }

  protected copyLink() {
    const url = this.inviteUrl();
    if (url) {
      navigator.clipboard.writeText(url);
    }
  }

  private async createInvite() {
    const invite = await firstValueFrom(this._userInvitesService.createInvite());
    const url = new URL(`../../invite/${invite.id}`, window.location.href);
    this.inviteUrl.set(url.href);
    this.inviteExpiresAt.set(new Date(invite.expiresAt));
  }
}
