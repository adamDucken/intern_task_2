import { Routes } from '@angular/router';
import { AllHousesComponent } from './components/all-houses/all-houses.component';
import { HouseDetailComponent } from './components/house-detail/house-detail.component';
import { ApartmentDetailComponent } from './components/apartment-detail/apartment-detail.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/all-houses', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'all-houses', component: AllHousesComponent, canActivate: [authGuard] },
  { path: 'house/:id', component: HouseDetailComponent, canActivate: [authGuard] },
  { path: 'apartment/:id', component: ApartmentDetailComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/all-houses' }
];
