import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  signal,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputMaskModule } from 'primeng/inputmask';
import { InputTextModule } from 'primeng/inputtext';

import { updateEventAction } from '../../../../+state/events';
import { Event } from '../../../../models/parsed-models';
import { TranslateService } from '../../../../services/translate.service';

@Component({
  selector: 'app-modify-external-uri-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    DialogModule,
    FormsModule,
    InputGroupModule,
    InputMaskModule,
    InputTextModule,
  ],
  templateUrl: './modify-external-uri-dialog.component.html',
  styleUrl: './modify-external-uri-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModifyExternalUriDialogComponent {
  private readonly _store = inject(Store);
  private readonly _translateService = inject(TranslateService);

  protected readonly translations = this._translateService.translations;
  public readonly event = input.required<Event>();
  protected readonly visible = signal(false);
  protected readonly externalUri = signal('');

  @ViewChild('inputElement') inputElement!: ElementRef;

  public open(currentValue: string | null | undefined) {
    this.visible.set(true);
    this.externalUri.set(currentValue != null ? (currentValue as string) : '');
  }

  protected updateExternalUri() {
    const event = this.event();
    if (!event) return;

    this._store.dispatch(
      updateEventAction({
        eventId: event.id,
        externalUri: this.externalUri().length != 0 ? this.externalUri() : ('' as string),
      })
    );

    this.visible.set(false);
  }

  protected clearExternalUri() {
    this.externalUri.set('');
    this.inputElement.nativeElement.focus();
  }
}
