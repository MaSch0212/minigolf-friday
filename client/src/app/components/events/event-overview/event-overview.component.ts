import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-event-overview',
  standalone: true,
  imports: [],
  templateUrl: './event-overview.component.html',
  styleUrl: './event-overview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventOverviewComponent {

}
