import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';

import { FadingMessageComponent } from './fading-message.component';
import { TranslateService } from '../../services/translate.service';

@Component({
  selector: 'app-saved-fading-message',
  standalone: true,
  imports: [FadingMessageComponent],
  template: `
    <app-fading-message
      class="pointer-events-none absolute top-1/2 -translate-x-[calc(100%+8px)] -translate-y-1/2 truncate rounded bg-green-100 px-4 py-2 text-green-900 dark:bg-green-900 dark:text-green-200"
      [showTrigger]="showTrigger()"
    >
      <div class="flex flex-row items-center gap-1">
        <span class="i-[mdi--check]"></span>
        <ng-content>
          {{ translations.shared_saved() }}
        </ng-content>
      </div>
    </app-fading-message>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SavedFadingMessageComponent {
  protected readonly translations = inject(TranslateService).translations;

  public readonly showTrigger = input<boolean | null | undefined>();
}
