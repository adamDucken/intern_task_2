import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';

declare const google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  email = '';
  password = '';
  loading = false;
  error = '';

  constructor(private auth: AuthService, private router: Router) { }

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.router.navigate(['/all-houses']);
      return;
    }
    this.initGoogleSignIn();
  }

  initGoogleSignIn(): void {
    const interval = setInterval(() => {
      if (typeof google !== 'undefined' && google.accounts) {
        clearInterval(interval);
        google.accounts.id.initialize({
          client_id: this.getGoogleClientId(),
          callback: (response: any) => this.handleGoogleCredential(response)
        });
        google.accounts.id.renderButton(
          document.getElementById('google-btn'),
          { theme: 'outline', size: 'large', width: 300 }
        );
      }
    }, 100);
  }

  getGoogleClientId(): string {
    return environment.googleClientId;
  }

  handleGoogleCredential(response: any): void {
    this.loading = true;
    this.error = '';
    this.auth.googleLogin(response.credential).subscribe({
      next: () => this.router.navigate(['/all-houses']),
      error: (err) => {
        this.error = err.error?.message || 'google login failed';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.error = 'email and password are required';
      return;
    }

    this.loading = true;
    this.error = '';

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => this.router.navigate(['/all-houses']),
      error: (err) => {
        this.error = err.error?.message || 'invalid email or password';
        this.loading = false;
      }
    });
  }
}
