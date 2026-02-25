import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  email = '';
  password = '';
  role = 'Resident';
  loading = false;
  error = '';

  constructor(private auth: AuthService, private router: Router) { }

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.error = 'email and password are required';
      return;
    }

    this.loading = true;
    this.error = '';

    this.auth.register({ email: this.email, password: this.password, role: this.role }).subscribe({
      next: () => this.router.navigate(['/all-houses']),
      error: (err) => {
        this.error = err.error?.message || 'registration failed';
        this.loading = false;
      }
    });
  }
}
