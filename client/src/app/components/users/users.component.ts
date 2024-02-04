import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';

import { TranslateService } from '../../services/translate.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [ButtonModule, CommonModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsersComponent {
  private readonly _translateService = inject(TranslateService);

  protected translations = this._translateService.translations;
}
