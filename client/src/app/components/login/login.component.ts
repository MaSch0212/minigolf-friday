import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { TooltipModule } from 'primeng/tooltip';
import { map, startWith } from 'rxjs';

import { ErrorTextDirective } from '../../directives/error-text.directive';
import { InterpolatePipe } from '../../directives/interpolate.pipe';
import { AuthService } from '../../services/auth.service';
import { TranslateService } from '../../services/translate.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    CommonModule,
    DividerModule,
    ErrorTextDirective,
    ReactiveFormsModule,
    InterpolatePipe,
    InputTextModule,
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
  protected readonly isRegister = signal(false);

  protected readonly loginForm = this._formBuilder.group({
    email: this._formBuilder.control<string>('', {
      nonNullable: true,
      updateOn: 'blur',
      validators: [Validators.required],
    }),
    password: this._formBuilder.control<string>('', {
      nonNullable: true,
      updateOn: 'blur',
      validators: [Validators.required],
    }),
  });

  protected readonly registerForm = this._formBuilder.group({
    email: this._formBuilder.control<string>('', {
      nonNullable: true,
      updateOn: 'blur',
      validators: [Validators.required, Validators.email],
    }),
    name: this._formBuilder.control<string>('', {
      nonNullable: true,
      updateOn: 'blur',
      validators: [Validators.required],
    }),
    password: this._formBuilder.control<string>('', {
      nonNullable: true,
      updateOn: 'blur',
      validators: [
        Validators.required,
        Validators.minLength(6),
        passwordsEqualValidator('confirmPassword'),
      ],
    }),
    confirmPassword: this._formBuilder.control<string>('', {
      nonNullable: true,
      updateOn: 'blur',
      validators: [Validators.required, passwordsEqualValidator('password')],
    }),
  });

  constructor() {
    const regPw = toSignal(this.registerForm.controls.password.valueChanges);
    const regCp = toSignal(this.registerForm.controls.confirmPassword.valueChanges);

    effect(
      () => {
        regPw();
        this.registerForm.controls.confirmPassword.updateValueAndValidity();
      },
      { allowSignalWrites: true }
    );

    effect(
      () => {
        regCp();
        this.registerForm.controls.password.updateValueAndValidity();
      },
      { allowSignalWrites: true }
    );

    effect(() => {
      if (this._authService.isAuthorized()) {
        this._router.navigate([this._returnUrl()]);
      }
    });
  }

  protected loginWithFacebook(): void {
    // TODO: Error handling
    this._authService.signIn('facebook');
  }

  protected loginByEmail(): void {
    this.loginForm.updateValueAndValidity();
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { email, password } = this.loginForm.value as { email: string; password: string };

    // TODO: Error handling
    this._authService.signIn('email', email, password);
  }

  protected register(): void {
    this.registerForm.updateValueAndValidity();
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const { email, name, password } = this.registerForm.value as {
      email: string;
      name: string;
      password: string;
    };

    // TODO: Error handling
    this._authService.register(email, name, password);
  }
}

function passwordsEqualValidator(otherControlName: string): ValidatorFn {
  return control => {
    const otherControl = control.parent?.get(otherControlName) as AbstractControl;
    if (!otherControl) {
      return null;
    }

    if (control.value && otherControl.value && control.value !== otherControl.value) {
      return { passwordsEqual: true };
    }

    return null;
  };
}
