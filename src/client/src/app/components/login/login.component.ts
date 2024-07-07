import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { distinctUntilChanged, map, startWith } from 'rxjs';

import { ErrorTextDirective } from '../../directives/error-text.directive';
import { InterpolatePipe } from '../../directives/interpolate.pipe';
import { OnEnterDirective } from '../../directives/on-enter.directive';
import { AuthService, SignInResult } from '../../services/auth.service';
import { Logger } from '../../services/logger.service';
import { TranslateService } from '../../services/translate.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    CommonModule,
    ErrorTextDirective,
    OnEnterDirective,
    ReactiveFormsModule,
    ProgressSpinnerModule,
    InputTextModule,
    InterpolatePipe,
    PasswordModule,
    TooltipModule,
  ],
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _formBuilder = inject(FormBuilder);

  private readonly _returnUrl = toSignal(
    this._activatedRoute.queryParams.pipe(
      map(params => params['returnUrl'] ?? '/'),
      startWith('/')
    )
  );

  protected readonly translations = inject(TranslateService).translations;
  protected readonly isAuthInitialized = computed(() => this._authService.token() !== undefined);
  protected readonly isLoggingIn = signal(false);
  protected readonly loginResult = signal<SignInResult | null>(null);

  protected readonly loginForm = this._formBuilder.group({
    loginToken: this._formBuilder.control<string>('', {
      nonNullable: true,
      updateOn: 'change',
      validators: [Validators.required],
    }),
  });

  constructor() {
    this.loginForm.valueChanges
      .pipe(
        takeUntilDestroyed(),
        distinctUntilChanged((a, b) => a.loginToken === b.loginToken)
      )
      .subscribe(() => this.loginResult.set(null));

    effect(
      () => {
        if (this.isLoggingIn()) {
          this.loginForm.disable();
        } else {
          this.loginForm.enable();
        }
      },
      { allowSignalWrites: true }
    );

    effect(
      () => {
        if (this._authService.isAuthorized()) {
          this._router.navigate([this._returnUrl()]);
        }
      },
      { allowSignalWrites: true }
    );
  }

  protected async login(): Promise<void> {
    this.loginForm.updateValueAndValidity();
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoggingIn.set(true);
    this.loginResult.set(null);
    try {
      const loginToken = this.loginForm.value.loginToken ?? '';
      const result = await this._authService.signIn(loginToken);
      this.loginResult.set(result);
    } catch (error) {
      Logger.logError('LoginComponent', 'Failed to sign in', error);
      this.loginResult.set('error');
    } finally {
      this.isLoggingIn.set(false);
    }
  }
}
