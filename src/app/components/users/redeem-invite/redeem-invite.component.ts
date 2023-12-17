import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { AuthService } from '../../../services/auth.service';
import { UserInvitesService } from '../../../services/user-invites.service';

@Component({
  selector: 'app-redeem-invite',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './redeem-invite.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RedeemInviteComponent {
  private readonly _userInvitesService = inject(UserInvitesService);
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);

  constructor() {
    const id = inject(ActivatedRoute).snapshot.params['inviteId'];
    if (id) {
      this._userInvitesService.redeemInvite(id).subscribe(() => {
        this._authService.refreshAuthorized();
        this._router.navigate(['/home']);
      });
    }
  }
}
