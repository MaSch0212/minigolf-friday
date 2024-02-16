import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

import { EventOverviewComponent } from '../events/event-overview/event-overview.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, EventOverviewComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {}
