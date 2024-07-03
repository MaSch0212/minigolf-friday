import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AutoFocusModule } from 'primeng/autofocus';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';

import { NotificationsService } from '../../../api/services';
import { InterpolatePipe } from '../../../directives/interpolate.pipe';
import { User } from '../../../models/parsed-models';
import { TranslateService } from '../../../services/translate.service';

@Component({
  selector: 'app-user-push-dialog',
  standalone: true,
  imports: [
    AutoFocusModule,
    ButtonModule,
    CommonModule,
    DialogModule,
    InputTextModule,
    InputTextareaModule,
    InterpolatePipe,
    ReactiveFormsModule,
  ],
  templateUrl: './user-push-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserPushDialogComponent {
  protected readonly translations = inject(TranslateService).translations;
  private readonly _randomId = Math.random().toString(36).substring(2, 9);

  private readonly _formBuilder = inject(FormBuilder);
  private readonly _notificationsService = inject(NotificationsService);

  protected readonly visible = signal(false);
  protected readonly isLoading = signal(false);
  protected readonly user = signal<User | undefined>(undefined);
  protected readonly form = this._formBuilder.group({
    title: this._formBuilder.control(''),
    body: this._formBuilder.control(''),
  });

  public open(user: User): void {
    this.user.set(user);
    this.visible.set(true);
  }

  public close(): void {
    this.visible.set(false);
  }

  protected async submit() {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    const user = this.user();
    if (!user) return;

    this.isLoading.set(true);
    try {
      await this._notificationsService.sendNotification({
        body: {
          userId: user.id,
          title: this.form.value.title || this.translations.users_notificationDialog_titleDefault(),
          body: this.form.value.body || this.translations.users_notificationDialog_bodyDefault(),
        },
      });
      this.close();
    } finally {
      this.isLoading.set(false);
    }
  }

  protected id(purpose: string) {
    return `${purpose}-${this._randomId}`;
  }
}
